using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Application.Abstractions.Repositories
{
    public interface ITeamRepository
    {
        Task<List<Team>> ListAsync();
        Task<Team?> GetAsync(Guid id);
        Task AddAsync(Team team);
        Task UpdateAsync(Team team);
        Task DeleteAsync(Team team);
    }
}

