
using Microsoft.AspNetCore.RateLimiting;
using Espadium.Wiki.Api.Configuration.Options;
using Espadium.Wiki.Infrastructure;
using Espadium.Wiki.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Threading.RateLimiting;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using System.IdentityModel.Tokens.Jwt;

namespace Espadium.Wiki.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(formatter: new Serilog.Formatting.Json.JsonFormatter())
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .MinimumLevel.Is(LogEventLevel.Information)
                    .WriteTo.Console(formatter: new Serilog.Formatting.Json.JsonFormatter());
            });

            builder.Services.AddAuthorization();
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Espadium.Wiki API", Version = "v1" });
                var scheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer {token}'"
                };
                c.AddSecurityDefinition("Bearer", scheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { scheme, Array.Empty<string>() }
                });
            });

            builder.Services.AddDbContext<WikiDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

            builder.Services
                .AddIdentityCore<User>(o =>
                {
                    o.User.RequireUniqueEmail = true;
                    o.SignIn.RequireConfirmedEmail = false;
                })
                .AddRoles<Role>()
                .AddEntityFrameworkStores<WikiDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
                    };
                });

            builder.Services
                .AddOptions<JwtOptions>()
                .Bind(builder.Configuration.GetSection("Jwt"))
                .ValidateDataAnnotations()
                .Validate(o => !string.IsNullOrWhiteSpace(o.Key) && o.Key.Length >= 16, "Jwt:Key must be at least 16 characters")
                .ValidateOnStart();

            builder.Services
                .AddOptions<S3Options>()
                .Bind(builder.Configuration.GetSection("S3"))
                .ValidateDataAnnotations()
                .Validate(o => Uri.TryCreate(o.Endpoint, UriKind.Absolute, out _), "S3:Endpoint must be a valid absolute URI")
                .ValidateOnStart();

            builder.Services
                .AddOptions<EmailOptions>()
                .Bind(builder.Configuration.GetSection("Email"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddOptions<StorageLimitsOptions>()
                .Bind(builder.Configuration.GetSection("StorageLimits"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddOptions<ContentLimitsOptions>()
                .Bind(builder.Configuration.GetSection("ContentLimits"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddOptions<SecurityOptions>()
                .Bind(builder.Configuration.GetSection("Security"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 60,
                            Window = TimeSpan.FromMinutes(1)
                        }));
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
                          .AllowCredentials()
                          .AllowAnyHeader();
                });
            });

            WebApplication app;
            try
            {
                app = builder.Build();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to start due to configuration validation errors");
                throw;
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseCors();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/health/liveness", () => Results.Json(new { status = "ok" }));
            app.MapGet("/health/readiness", () => Results.Json(new { db = "pending", redis = "pending", s3 = "pending" }));

            var auth = app.MapGroup("/auth");
            auth.WithOpenApi().WithTags("Auth");

            auth.MapPost("/register", async (
                UserManager<User> userManager,
                HttpContext http,
                RegisterRequest req) =>
            {
                var user = new User { Id = Guid.NewGuid(), UserName = req.Email, Email = req.Email };
                var result = await userManager.CreateAsync(user, req.Password);
                if (!result.Succeeded)
                {
                    return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));
                }
                return Results.Ok(new { message = "registered" });
            });

            auth.MapPost("/login", async (
                UserManager<User> userManager,
                WikiDbContext db,
                Microsoft.Extensions.Options.IOptions<JwtOptions> jwtOptions,
                HttpContext http,
                LoginRequest req) =>
            {
                var user = await userManager.FindByEmailAsync(req.Email);
                if (user == null || !await userManager.CheckPasswordAsync(user, req.Password))
                {
                    return Results.Unauthorized();
                }

                var accessToken = GenerateJwt(user, jwtOptions.Value);
                var refresh = new Infrastructure.Models.RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(14)
                };
                db.RefreshTokens.Add(refresh);
                await db.SaveChangesAsync();

                SetRefreshCookie(http, refresh.Token);
                return Results.Ok(new { accessToken });
            });

            auth.MapPost("/refresh", async (
                WikiDbContext db,
                Microsoft.Extensions.Options.IOptions<JwtOptions> jwtOptions,
                HttpContext http) =>
            {
                if (!http.Request.Cookies.TryGetValue("refresh_token", out var token))
                {
                    return Results.Unauthorized();
                }
                var rt = await db.RefreshTokens.Include(r => r.User).FirstOrDefaultAsync(r => r.Token == token);
                if (rt == null || rt.RevokedAt != null || rt.ExpiresAt <= DateTimeOffset.UtcNow || rt.User == null)
                {
                    return Results.Unauthorized();
                }

                rt.RevokedAt = DateTimeOffset.UtcNow;
                var newRt = new Infrastructure.Models.RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = rt.UserId,
                    Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(14)
                };
                db.RefreshTokens.Add(newRt);
                await db.SaveChangesAsync();

                SetRefreshCookie(http, newRt.Token);
                var accessToken = GenerateJwt(rt.User, jwtOptions.Value);
                return Results.Ok(new { accessToken });
            });

            auth.MapPost("/logout", async (WikiDbContext db, HttpContext http) =>
            {
                if (http.Request.Cookies.TryGetValue("refresh_token", out var token))
                {
                    var tokens = db.RefreshTokens.Where(r => r.Token == token);
                    await tokens.ForEachAsync(r => r.RevokedAt = DateTimeOffset.UtcNow);
                    await db.SaveChangesAsync();
                }
                http.Response.Cookies.Delete("refresh_token");
                return Results.Ok(new { message = "logged out" });
            }).RequireAuthorization();

            auth.MapPost("/forgot-password", async (UserManager<User> userManager, ForgotPasswordRequest req) =>
            {
                var user = await userManager.FindByEmailAsync(req.Email);
                if (user != null)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                }
                return Results.Ok(new { message = "ok" });
            });

            auth.MapPost("/reset-password", async (UserManager<User> userManager, ResetPasswordRequest req) =>
            {
                var user = await userManager.FindByEmailAsync(req.Email);
                if (user == null)
                {
                    return Results.BadRequest(new { error = "invalid_user" });
                }
                var result = await userManager.ResetPasswordAsync(user, req.ResetCode, req.NewPassword);
                if (!result.Succeeded)
                {
                    return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));
                }
                return Results.Ok(new { message = "ok" });
            });

            auth.MapGet("/confirm-email", async (UserManager<User> userManager, Guid userId, string code) =>
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null) return Results.BadRequest(new { error = "invalid_user" });
                var result = await userManager.ConfirmEmailAsync(user, code);
                return result.Succeeded ? Results.Ok(new { message = "ok" }) : Results.BadRequest(result.Errors);
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.Run();
        }

        private static string GenerateJwt(User user, JwtOptions jwt)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: jwt.Issuer,
                audience: jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwt.AccessTokenMinutes),
                signingCredentials: creds);
            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }

        private static void SetRefreshCookie(HttpContext http, string token)
        {
            http.Response.Cookies.Append("refresh_token", token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = false,
                Expires = DateTimeOffset.UtcNow.AddDays(14),
                Path = "/"
            });
        }
    }
}
