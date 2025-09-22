
using Microsoft.AspNetCore.RateLimiting;
using Espadium.Wiki.Api.Configuration.Options;
using Serilog;
using Serilog.Events;
using System.Threading.RateLimiting;

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
            }

            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseCors();
            app.UseRateLimiter();
            app.UseAuthorization();

            app.MapGet("/health/liveness", () => Results.Json(new { status = "ok" }));
            app.MapGet("/health/readiness", () => Results.Json(new { db = "pending", redis = "pending", s3 = "pending" }));

            app.Run();
        }
    }
}
