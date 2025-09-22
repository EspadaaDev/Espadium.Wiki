namespace Espadium.Wiki.Application.Search
{
    public record SearchRequest(string Q, Guid? SpaceId = null, int Limit = 20, int Offset = 0);
    public record SearchResultItem(Guid PageId, Guid SpaceId, string Title, string Snippet, double Rank, DateTimeOffset UpdatedAt);

    public interface IPageSearchService
    {
        Task<IReadOnlyList<SearchResultItem>> SearchAsync(SearchRequest req, CancellationToken ct);
    }
}

