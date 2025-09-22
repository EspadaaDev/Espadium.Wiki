using Espadium.Wiki.Application.Abstractions.Caching;

namespace Espadium.Wiki.Infrastructure.Caching
{
    public class SettingsCache : ISettingsCache
    {
        private readonly ICacheService _cache;
        public SettingsCache(ICacheService cache) { _cache = cache; }

        private const string KeyName = "ew:settings";

        public Task<string?> GetAsync(CancellationToken ct = default)
            => _cache.GetAsync<string>(KeyName, ct);

        public Task SetAsync(string settingsJson, TimeSpan? ttl = null, CancellationToken ct = default)
            => _cache.SetAsync(KeyName, settingsJson, ttl ?? TimeSpan.FromSeconds(60), ct);

        public Task InvalidateAsync(CancellationToken ct = default)
            => _cache.RemoveAsync(KeyName, ct);
    }
}

