using Espadium.Wiki.Application.Abstractions.Caching;
using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Infrastructure.Caching
{
    public class PageCache : IPageCache
    {
        private readonly ICacheService _cache;
        public PageCache(ICacheService cache) { _cache = cache; }

        private static string Key(Guid id, int? rev) => rev.HasValue ? $"ew:page:{id}:v{rev.Value}" : $"ew:page:{id}";

        public Task<Page?> GetPageAsync(Guid pageId, int? revision = null, CancellationToken ct = default)
            => _cache.GetAsync<Page>(Key(pageId, revision), ct);

        public Task SetPageAsync(Page page, int? revision = null, TimeSpan? ttl = null, CancellationToken ct = default)
            => _cache.SetAsync(Key(page.Id, revision), page, ttl ?? TimeSpan.FromSeconds(300), ct);

        public Task InvalidatePageAsync(Guid pageId, int? revision = null, CancellationToken ct = default)
            => _cache.RemoveAsync(Key(pageId, revision), ct);
    }
}

