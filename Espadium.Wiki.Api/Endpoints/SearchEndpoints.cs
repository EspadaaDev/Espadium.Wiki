using Espadium.Wiki.Application.Search;

namespace Espadium.Wiki.Api.Endpoints
{
    public static class SearchEndpoints
    {
        public static IEndpointRouteBuilder MapSearch(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/search").RequireAuthorization();
            group.MapGet("/", async (HttpContext http, IPageSearchService search, string q, Guid? spaceId, int? limit, int? offset, CancellationToken ct) =>
            {
                var l = Math.Clamp(limit ?? 20, 1, 50);
                var o = Math.Max(offset ?? 0, 0);
                var req = new SearchRequest(q ?? string.Empty, spaceId, l, o);
                var items = await search.SearchAsync(req, ct);
                return Results.Ok(items);
            });
            return app;
        }
    }
}

