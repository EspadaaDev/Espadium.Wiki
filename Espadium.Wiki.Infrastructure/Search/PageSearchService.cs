using Espadium.Wiki.Application.Search;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Infrastructure.Search
{
    public class PageSearchService : IPageSearchService
    {
        private readonly WikiDbContext _db;
        public PageSearchService(WikiDbContext db) { _db = db; }

        public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(SearchRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Q) || req.Q.Trim().Length < 2) return Array.Empty<SearchResultItem>();

            var q = req.Q.Trim();
            var space = req.SpaceId;
            var limit = Math.Clamp(req.Limit, 1, 50);
            var offset = Math.Max(req.Offset, 0);

            var results = await _db.Set<SearchRow>().FromSqlInterpolated($@"
WITH q AS (SELECT websearch_to_tsquery('simple', {q}) AS query)
SELECT ps.page_id, ps.space_id, p.title,
       substring(ps.body_text from 1 for 160) AS snippet,
       ts_rank(ps.title_tsv, q.query) * 1.5 + ts_rank(ps.body_tsv, q.query) AS rank,
       p.updated_at
FROM page_search ps
JOIN pages p ON p.id = ps.page_id
CROSS JOIN q
WHERE (ps.title_tsv @@ q.query OR ps.body_tsv @@ q.query)
  AND ({space} IS NULL OR ps.space_id = {space})
ORDER BY rank DESC, p.updated_at DESC
LIMIT {limit} OFFSET {offset}")
            .AsNoTracking()
            .ToListAsync(ct);

            return results.Select(r => new SearchResultItem(r.page_id, r.space_id, r.title ?? string.Empty, r.snippet ?? string.Empty, r.rank, r.updated_at)).ToList();
        }

        private class SearchRow
        {
            public Guid page_id { get; set; }
            public Guid space_id { get; set; }
            public string? title { get; set; }
            public string? snippet { get; set; }
            public double rank { get; set; }
            public DateTimeOffset updated_at { get; set; }
        }
    }
}

