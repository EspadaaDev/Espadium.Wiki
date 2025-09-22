using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Application.Abstractions.Repositories
{
    public interface ISpaceRepository
    {
        Task<List<Space>> ListAsync();
        Task<Space?> GetAsync(Guid id);
        Task AddAsync(Space space);
        Task UpdateAsync(Space space);
        Task DeleteAsync(Space space);
        Task<bool> ExistsMemberAsync(Guid spaceId, PrincipalType type, Guid principalId);
        Task<SpaceRole?> GetUserRoleAsync(Guid spaceId, Guid userId);
        Task AddMemberAsync(SpaceMember member);
    }
}

