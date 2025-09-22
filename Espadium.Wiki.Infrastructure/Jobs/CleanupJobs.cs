using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Infrastructure.Jobs
{
    public class CleanupJobs
    {
        private readonly WikiDbContext _db;
        public CleanupJobs(WikiDbContext db) { _db = db; }

        public Task CleanupExpiredPublicLinksAsync(CancellationToken ct)
        {
            // TODO: implement when public links table exists
            return Task.CompletedTask;
        }

        public async Task CleanupStalePendingAttachmentsAsync(CancellationToken ct)
        {
            var cutoff = DateTimeOffset.UtcNow.AddHours(-24);
            var stale = await _db.Attachments.Where(a => a.Status == "pending" && a.CreatedAt < cutoff).ToListAsync(ct);
            if (stale.Count == 0) return;
            _db.Attachments.RemoveRange(stale);
            await _db.SaveChangesAsync(ct);
        }
    }
}

