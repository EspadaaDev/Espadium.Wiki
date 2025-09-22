using Espadium.Wiki.Application.Abstractions.Previews;
using Espadium.Wiki.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Infrastructure.Jobs
{
    public class AttachmentPreviewJob
    {
        private readonly WikiDbContext _db;
        private readonly IImagePreviewGenerator _generator;

        public AttachmentPreviewJob(WikiDbContext db, IImagePreviewGenerator generator)
        {
            _db = db;
            _generator = generator;
        }

        public async Task HandleAsync(Guid attachmentId, CancellationToken ct)
        {
            var att = await _db.Attachments.FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
            if (att == null) return;
            if (!att.Mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) return;
            if (!string.Equals(att.Status, "active", StringComparison.OrdinalIgnoreCase)) return;
            if (string.Equals(att.AvScan, "infected", StringComparison.OrdinalIgnoreCase)) return;
            if (!string.IsNullOrEmpty(att.PreviewKey)) return;
            var key = await _generator.GenerateAsync(att, ct);
            if (!string.IsNullOrEmpty(key))
            {
                att.PreviewKey = key;
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}

