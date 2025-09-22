using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Application.Abstractions.Repositories
{
    public interface IPageRestrictionRepository
    {
        Task<PageRestriction?> GetAsync(Guid pageId, Guid userId);
        Task AddAsync(PageRestriction restriction);
    }
}

