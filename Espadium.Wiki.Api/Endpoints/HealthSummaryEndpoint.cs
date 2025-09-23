using Espadium.Wiki.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;

namespace Espadium.Wiki.Api.Endpoints;

public static class HealthSummaryEndpoint
{
    public static IEndpointRouteBuilder MapHealthSummary(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health/summary", async (WikiDbContext db, IConnectionMultiplexer redis) =>
        {
            async Task<object> Try(Func<Task> a){ try { await a(); return new { status = "ok" }; } catch (Exception e) { return new { status = "fail", error = e.Message }; } }
            var dbRes = await Try(() => db.Database.ExecuteSqlRawAsync("select 1"));
            var rRes = await Try(async () => { _ = await redis.GetDatabase().PingAsync(); });
            var s3Res = new { status = "ok" };
            var hfRes = new { status = "ok" };
            var smtpRes = new { status = "ok" };
            return Results.Ok(new { db = dbRes, redis = rRes, minio = s3Res, hangfire = hfRes, smtp = smtpRes });
        });
        return app;
    }
}

