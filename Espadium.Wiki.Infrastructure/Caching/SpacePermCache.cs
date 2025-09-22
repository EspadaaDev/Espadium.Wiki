using Espadium.Wiki.Application.Abstractions.Caching;
using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Infrastructure.Caching
{
    public class SpacePermCache : ISpacePermCache
    {
        private readonly ICacheService _cache;
        public SpacePermCache(ICacheService cache) { _cache = cache; }

        private static string Key(Guid spaceId, Guid userId) => $"ew:spaceperm:{spaceId}:{userId}";

        public Task<SpaceRole?> GetAsync(Guid userId, Guid spaceId, CancellationToken ct = default)
            => _cache.GetAsync<SpaceRole?>(Key(spaceId, userId), ct);

        public Task SetAsync(Guid userId, Guid spaceId, SpaceRole role, TimeSpan? ttl = null, CancellationToken ct = default)
            => _cache.SetAsync(Key(spaceId, userId), role, ttl ?? TimeSpan.FromSeconds(60), ct);

        public Task InvalidateAsync(Guid userId, Guid spaceId, CancellationToken ct = default)
            => _cache.RemoveAsync(Key(spaceId, userId), ct);
    }
}

