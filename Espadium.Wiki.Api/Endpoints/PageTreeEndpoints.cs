using Espadium.Wiki.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Api.Endpoints;

public static class PageTreeEndpoints
{
    private record Node(Guid Id, string Title)
    {
        public List<Node> Children { get; } = new();
    }

    public static IEndpointRouteBuilder MapPageTree(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/spaces/{spaceId:guid}/pages/tree", async (Guid spaceId, WikiDbContext db) =>
        {
            var pages = await db.Pages.AsNoTracking()
                .Where(p => p.SpaceId == spaceId)
                .Select(p => new { p.Id, p.ParentId, p.Title })
                .ToListAsync();

            var dict = pages.ToDictionary(p => p.Id, p => new Node(p.Id, p.Title));
            foreach (var p in pages)
            {
                if (p.ParentId is Guid parent && dict.TryGetValue(parent, out var parentNode))
                    parentNode.Children.Add(dict[p.Id]);
            }
            var roots = pages.Where(p => p.ParentId == null).Select(p => dict[p.Id]).ToList();
            return Results.Ok(roots);
        })
        .RequireAuthorization()
        .WithTags("Pages");

        return app;
    }
}

