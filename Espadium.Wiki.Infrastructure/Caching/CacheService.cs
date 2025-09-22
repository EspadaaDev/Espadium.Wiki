using Espadium.Wiki.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Espadium.Wiki.Infrastructure.Caching
{
    public class CacheService : ICacheService
    {
        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web) { WriteIndented = false };
        private readonly IDistributedCache _cache;
        public CacheService(IDistributedCache cache) { _cache = cache; }

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            var bytes = await _cache.GetAsync(key, ct);
            if (bytes == null) return default;
            return JsonSerializer.Deserialize<T>(bytes, JsonOpts);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOpts);
            var opt = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };
            return _cache.SetAsync(key, bytes, opt, ct);
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
            => _cache.RemoveAsync(key, ct);
    }
}

