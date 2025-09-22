using System.Text.RegularExpressions;
using Espadium.Wiki.Application.Abstractions;
using Espadium.Wiki.Api.Configuration.Options;
using Microsoft.Extensions.Options;
using Espadium.Wiki.Domain.Entities;
using Espadium.Wiki.Infrastructure;
using Espadium.Wiki.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Api.Endpoints
{
    public static class AttachmentsEndpoints
    {
        public static IEndpointRouteBuilder MapAttachments(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api");

            group.MapPost("/pages/{pageId:guid}/attachments:init", async (
                Guid pageId,
                HttpContext http,
                IOptions<StorageLimitsOptions> limits,
                IFileStorage storage,
                WikiDbContext db,
                ISpaceRepository spaces,
                InitDto dto) =>
            {
                var userId = Guid.Parse(http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                var page = await db.Pages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pageId);
                if (page == null) return Results.NotFound();
                var role = await spaces.GetUserRoleAsync(page.SpaceId, userId);
                if (role is null || (role != SpaceRole.Admin && role != SpaceRole.Contributor)) return Results.Forbid();

                if (dto.SizeBytes > limits.Value.MaxFileSizeMB * 1024L * 1024L) return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
                if (!limits.Value.AllowedMimeTypes.Any(m => Matches(dto.Mime, m))) return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);

                if (!limits.Value.UsePresignedUrls) return Results.StatusCode(StatusCodes.Status501NotImplemented);

                var safeName = Sanitize(dto.Filename);
                var key = $"{pageId}/{Guid.NewGuid()}/{safeName}";
                var init = await storage.InitAsync(key, dto.SizeBytes, limits.Value.MultipartUploadThresholdMB * 1024 * 1024);

                var attachment = new Attachment
                {
                    Id = Guid.NewGuid(),
                    PageId = pageId,
                    StorageKey = key,
                    Bucket = "", // optional display only
                    Filename = safeName,
                    Mime = dto.Mime,
                    Size = dto.SizeBytes,
                    Sha256 = dto.Sha256,
                    Status = "pending",
                    AvScan = "pending",
                    CreatedBy = userId,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                db.Attachments.Add(attachment);
                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    attachmentId = attachment.Id,
                    uploadId = init.UploadId,
                    partSizeBytes = init.PartSizeBytes,
                    presignedUrls = init.Parts.Select(p => new { p.PartNumber, p.Url })
                });
            }).RequireAuthorization();

            group.MapPost("/pages/{pageId:guid}/attachments:complete", async (
                Guid pageId,
                HttpContext http,
                IFileStorage storage,
                WikiDbContext db,
                ISpaceRepository spaces,
                CompleteDto dto) =>
            {
                var userId = Guid.Parse(http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                var page = await db.Pages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pageId);
                if (page == null) return Results.NotFound();
                var role = await spaces.GetUserRoleAsync(page.SpaceId, userId);
                if (role is null || (role != SpaceRole.Admin && role != SpaceRole.Contributor)) return Results.Forbid();

                var att = await db.Attachments.FirstOrDefaultAsync(a => a.Id == dto.AttachmentId && a.PageId == pageId);
                if (att == null) return Results.NotFound();

                await storage.CompleteAsync(att.StorageKey, dto.UploadId, dto.Parts.Select(p => (p.PartNumber, p.ETag)));
                att.Status = "active";
                await db.SaveChangesAsync();
                return Results.Ok(new { att.Id, att.Filename, att.Mime, att.Size, att.Status });
            }).RequireAuthorization();

            group.MapGet("/pages/{pageId:guid}/attachments", async (Guid pageId, WikiDbContext db) =>
            {
                var list = await db.Attachments.AsNoTracking().Where(a => a.PageId == pageId && a.Status == "active")
                    .Select(a => new { a.Id, a.Filename, a.Mime, a.Size, a.Status }).ToListAsync();
                return Results.Ok(list);
            }).RequireAuthorization();

            group.MapDelete("/attachments/{id:guid}", async (Guid id, WikiDbContext db, IFileStorage storage) =>
            {
                var att = await db.Attachments.FirstOrDefaultAsync(a => a.Id == id);
                if (att == null) return Results.NotFound();
                try { await storage.DeleteAsync(att.StorageKey); } catch { }
                db.Attachments.Remove(att);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).RequireAuthorization();

            return app;
        }

        private static bool Matches(string mime, string pattern)
        {
            if (pattern.EndsWith("/*", StringComparison.OrdinalIgnoreCase))
                return mime.StartsWith(pattern[..^1], StringComparison.OrdinalIgnoreCase);
            if (pattern.Contains('*'))
            {
                var rx = new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$", RegexOptions.IgnoreCase);
                return rx.IsMatch(mime);
            }
            return string.Equals(mime, pattern, StringComparison.OrdinalIgnoreCase);
        }

        private static string Sanitize(string filename)
        {
            var cleaned = Regex.Replace(filename, "[^a-zA-Z0-9._-]", "_");
            return cleaned.Length > 256 ? cleaned[..256] : cleaned;
        }

        public record InitDto(string Filename, string Mime, long SizeBytes, string? Sha256);
        public record CompleteDto(Guid AttachmentId, string UploadId, IEnumerable<PartDto> Parts);
        public record PartDto(int PartNumber, string ETag);
    }
}

