using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Application.Abstractions.Caching
{
    public interface ISpacePermCache
    {
        Task<SpaceRole?> GetAsync(Guid userId, Guid spaceId, CancellationToken ct = default);
        Task SetAsync(Guid userId, Guid spaceId, SpaceRole role, TimeSpan? ttl = null, CancellationToken ct = default);
        Task InvalidateAsync(Guid userId, Guid spaceId, CancellationToken ct = default);
    }
}

