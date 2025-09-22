using Espadium.Wiki.Application.Abstractions.Settings;
using Espadium.Wiki.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Infrastructure.Settings
{
    public class SettingsStore : ISettingsStore
    {
        private readonly WikiDbContext _db;
        public SettingsStore(WikiDbContext db) { _db = db; }

        public async Task<Dictionary<string, string>> GetAllAsync(CancellationToken ct)
        {
            return await _db.SystemSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value, ct);
        }

        public async Task UpsertAsync(string key, string value, Guid userId, CancellationToken ct)
        {
            var existing = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
            var now = DateTimeOffset.UtcNow;
            if (existing == null)
            {
                _db.SystemSettings.Add(new SystemSetting { Key = key, Value = value, UpdatedAt = now, UpdatedBy = userId });
                _db.AdminSettingsAudits.Add(new AdminSettingsAudit { Id = Guid.NewGuid(), Key = key, OldValue = string.Empty, NewValue = value, ChangedAt = now, ChangedBy = userId });
            }
            else
            {
                if (existing.Value != value)
                {
                    _db.AdminSettingsAudits.Add(new AdminSettingsAudit { Id = Guid.NewGuid(), Key = key, OldValue = existing.Value, NewValue = value, ChangedAt = now, ChangedBy = userId });
                    existing.Value = value;
                    existing.UpdatedAt = now;
                    existing.UpdatedBy = userId;
                }
            }
            await _db.SaveChangesAsync(ct);
        }

        public async Task BulkUpsertAsync(Dictionary<string, string> kv, Guid userId, CancellationToken ct)
        {
            foreach (var (key, value) in kv)
            {
                await UpsertAsync(key, value, userId, ct);
            }
        }
    }
}

