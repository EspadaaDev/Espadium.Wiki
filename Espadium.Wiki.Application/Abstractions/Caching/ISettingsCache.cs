namespace Espadium.Wiki.Application.Abstractions.Caching
{
    public interface ISettingsCache
    {
        Task<string?> GetAsync(CancellationToken ct = default);
        Task SetAsync(string settingsJson, TimeSpan? ttl = null, CancellationToken ct = default);
        Task InvalidateAsync(CancellationToken ct = default);
    }
}

