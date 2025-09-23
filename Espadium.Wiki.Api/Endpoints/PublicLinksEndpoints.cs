using Espadium.Wiki.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace Espadium.Wiki.Api.Endpoints;

public static class PublicLinksEndpoints
{
    public static IEndpointRouteBuilder MapPublicLinks(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/pages/{pageId:guid}/public-link", async (Guid pageId, WikiDbContext db, HttpContext http, Espadium.Wiki.Api.Configuration.Options.SecurityOptions sec) =>
        {
            var page = await db.Pages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pageId);
            if (page == null) return Results.NotFound();

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
            var link = await db.PublicLinks.FirstOrDefaultAsync(l => l.PageId == pageId && !l.Revoked);

            if (link == null)
            {
                link = new Espadium.Wiki.Domain.Entities.PublicLink
                {
                    Id = Guid.NewGuid(),
                    PageId = pageId,
                    Token = token,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = Guid.Empty,
                    ExpiresAt = DateTimeOffset.UtcNow.AddHours(sec.DefaultPublicLinkTTLHours),
                    Revoked = false
                };
                db.Add(link);
            }
            else
            {
                link.Token = token;
                link.ExpiresAt = DateTimeOffset.UtcNow.AddHours(sec.DefaultPublicLinkTTLHours);
                link.Revoked = false;
            }

            await db.SaveChangesAsync();
            return Results.Ok(new { url = $"/p/{link.Token}" });
        }).RequireAuthorization();

        app.MapDelete("/api/pages/{pageId:guid}/public-link", async (Guid pageId, WikiDbContext db) =>
        {
            var link = await db.PublicLinks.FirstOrDefaultAsync(l => l.PageId == pageId && !l.Revoked);
            if (link == null) return Results.NotFound();
            link.Revoked = true;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        app.MapGet("/p/{token}", async (string token, WikiDbContext db) =>
        {
            var link = await db.PublicLinks.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Token == token && !l.Revoked && l.ExpiresAt > DateTimeOffset.UtcNow);
            if (link == null) return Results.NotFound();

            var page = await db.Pages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == link.PageId);
            if (page == null) return Results.NotFound();

            var atts = await db.Attachments.AsNoTracking()
                .Where(a => a.PageId == page.Id && a.Status == "active")
                .Select(a => new { a.Id, a.Filename, a.Mime, a.Size, a.PreviewKey })
                .ToListAsync();

            return Results.Ok(new { page = new { page.Id, page.Title, snapshotJson = page.SnapshotJson }, attachments = atts });
        });

        return app;
    }
}

