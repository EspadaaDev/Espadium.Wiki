namespace Espadium.Wiki.Application.Abstractions.Caching
{
    public interface IPageTreeCache
    {
        Task<string?> GetTreeAsync(Guid spaceId, CancellationToken ct = default);
        Task SetTreeAsync(Guid spaceId, string treeJson, TimeSpan? ttl = null, CancellationToken ct = default);
        Task InvalidateTreeAsync(Guid spaceId, CancellationToken ct = default);
    }
}

