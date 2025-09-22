using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Application.Abstractions.Caching
{
    public interface IPageCache
    {
        Task<Page?> GetPageAsync(Guid pageId, int? revision = null, CancellationToken ct = default);
        Task SetPageAsync(Page page, int? revision = null, TimeSpan? ttl = null, CancellationToken ct = default);
        Task InvalidatePageAsync(Guid pageId, int? revision = null, CancellationToken ct = default);
    }
}

