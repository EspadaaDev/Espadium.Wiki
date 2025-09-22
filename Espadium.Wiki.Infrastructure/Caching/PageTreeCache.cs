using Espadium.Wiki.Application.Abstractions.Caching;

namespace Espadium.Wiki.Infrastructure.Caching
{
    public class PageTreeCache : IPageTreeCache
    {
        private readonly ICacheService _cache;
        public PageTreeCache(ICacheService cache) { _cache = cache; }

        private static string Key(Guid spaceId) => $"ew:pagetree:{spaceId}";

        public Task<string?> GetTreeAsync(Guid spaceId, CancellationToken ct = default)
            => _cache.GetAsync<string>(Key(spaceId), ct);

        public Task SetTreeAsync(Guid spaceId, string treeJson, TimeSpan? ttl = null, CancellationToken ct = default)
            => _cache.SetAsync(Key(spaceId), treeJson, ttl ?? TimeSpan.FromSeconds(120), ct);

        public Task InvalidateTreeAsync(Guid spaceId, CancellationToken ct = default)
            => _cache.RemoveAsync(Key(spaceId), ct);
    }
}

