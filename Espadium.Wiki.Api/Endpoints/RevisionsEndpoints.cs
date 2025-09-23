using Espadium.Wiki.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Espadium.Wiki.Api.Endpoints;

public static class RevisionsEndpoints
{
    public static IEndpointRouteBuilder MapRevisions(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/pages/{pageId:guid}").RequireAuthorization();

        grp.MapGet("/revisions", async (Guid pageId, WikiDbContext db) =>
        {
            var revs = await db.PageRevisions.AsNoTracking()
                .Where(r => r.PageId == pageId)
                .OrderByDescending(r => r.RevisionNo)
                .Select(r => new { r.Id, r.RevisionNo, r.AuthorId, r.CreatedAt })
                .ToListAsync();

            return Results.Ok(revs);
        });

        grp.MapPost("/revisions", async (Guid pageId, WikiDbContext db, HttpContext http) =>
        {
            var page = await db.Pages.FirstOrDefaultAsync(p => p.Id == pageId);
            if (page == null) return Results.NotFound();

            var maxNo = await db.PageRevisions.Where(r => r.PageId == pageId).MaxAsync(r => (int?)r.RevisionNo) ?? 0;
            var userIdStr = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _ = Guid.TryParse(userIdStr, out var userId);

            db.PageRevisions.Add(new Domain.Entities.PageRevision
            {
                Id = Guid.NewGuid(),
                PageId = pageId,
                RevisionNo = maxNo + 1,
                AuthorId = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                SnapshotJson = page.SnapshotJson ?? "{}",
                DiffJson = null
            });

            await db.SaveChangesAsync();
            return Results.Ok();
        });

        grp.MapPatch("", async (Guid pageId, WikiDbContext db, HttpContext http, Espadium.Wiki.Api.Configuration.Options.ContentLimitsOptions limits, PagePatchDto dto) =>
        {
            var page = await db.Pages.FirstOrDefaultAsync(p => p.Id == pageId);
            if (page == null) return Results.NotFound();

            if (dto.Title is { Length: > 0 } && dto.Title.Length > limits.MaxTitleLength)
                return Results.BadRequest(new { error = "invalid_title" });

            if (dto.SnapshotJson != null && dto.SnapshotJson.Length > limits.MaxPageLengthChars)
                return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);

            if (dto.Title != null) page.Title = dto.Title;
            if (dto.SnapshotJson != null) page.SnapshotJson = dto.SnapshotJson;
            page.UpdatedAt = DateTimeOffset.UtcNow;

            var maxNo = await db.PageRevisions.Where(r => r.PageId == pageId).MaxAsync(r => (int?)r.RevisionNo) ?? 0;
            var userIdStr = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _ = Guid.TryParse(userIdStr, out var userId);

            db.PageRevisions.Add(new Domain.Entities.PageRevision
            {
                Id = Guid.NewGuid(),
                PageId = pageId,
                RevisionNo = maxNo + 1,
                AuthorId = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                SnapshotJson = page.SnapshotJson ?? "{}",
                DiffJson = null
            });

            await db.SaveChangesAsync();
            return Results.Ok(new { page.Id, page.Title, snapshotJson = page.SnapshotJson });
        });

        return app;
    }

    public record PagePatchDto(string? Title, string? SnapshotJson);
}

