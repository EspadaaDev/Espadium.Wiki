using Espadium.Wiki.Application.Abstractions.Settings;
using Espadium.Wiki.Application.Settings;

namespace Espadium.Wiki.Api.Endpoints
{
    public static class AdminSettingsEndpoints
    {
        public static IEndpointRouteBuilder MapAdminSettings(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/admin/settings").RequireAuthorization("SiteAdminOnly");

            group.MapGet("/", async (ISettingsService service, CancellationToken ct) => Results.Ok(await service.GetAsync(ct)));

            group.MapPut("/", async (ISettingsService service, UpdateSystemSettingsDto dto, HttpContext http, CancellationToken ct) =>
            {
                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdStr, out var userId)) return Results.Forbid();
                try
                {
                    var updated = await service.UpdateAsync(dto, userId, ct);
                    return Results.Ok(updated);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            return app;
        }
    }
}

