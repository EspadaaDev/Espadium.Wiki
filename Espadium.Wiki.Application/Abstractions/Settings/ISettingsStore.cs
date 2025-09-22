namespace Espadium.Wiki.Application.Abstractions.Settings
{
    public interface ISettingsStore
    {
        Task<Dictionary<string, string>> GetAllAsync(CancellationToken ct);
        Task UpsertAsync(string key, string value, Guid userId, CancellationToken ct);
        Task BulkUpsertAsync(Dictionary<string, string> kv, Guid userId, CancellationToken ct);
    }
}

