using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Application.Abstractions.Repositories
{
    public interface IPageRepository
    {
        Task<List<Page>> ListAsync(Guid? spaceId = null);
        Task<Page?> GetAsync(Guid id);
        Task AddAsync(Page page);
        Task UpdateAsync(Page page);
        Task DeleteAsync(Page page);
    }
}

