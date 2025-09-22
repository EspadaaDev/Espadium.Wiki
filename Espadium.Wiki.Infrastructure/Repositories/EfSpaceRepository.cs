using Espadium.Wiki.Application.Abstractions.Repositories;
using Espadium.Wiki.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Infrastructure.Repositories
{
    public class EfSpaceRepository : ISpaceRepository
    {
        private readonly WikiDbContext _db;
        public EfSpaceRepository(WikiDbContext db) { _db = db; }

        public async Task AddAsync(Space space)
        {
            _db.Spaces.Add(space);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Space space)
        {
            _db.Spaces.Remove(space);
            await _db.SaveChangesAsync();
        }

        public Task<Space?> GetAsync(Guid id) => _db.Spaces.FindAsync(id).AsTask();

        public Task<List<Space>> ListAsync() => _db.Spaces.AsNoTracking().ToListAsync();

        public async Task UpdateAsync(Space space)
        {
            _db.Spaces.Update(space);
            await _db.SaveChangesAsync();
        }

        public Task<bool> ExistsMemberAsync(Guid spaceId, PrincipalType type, Guid principalId)
            => _db.SpaceMembers.AnyAsync(sm => sm.SpaceId == spaceId && sm.PrincipalType == type && sm.PrincipalId == principalId);

        public async Task<SpaceRole?> GetUserRoleAsync(Guid spaceId, Guid userId)
        {
            var m = await _db.SpaceMembers.AsNoTracking().FirstOrDefaultAsync(x => x.SpaceId == spaceId && x.PrincipalType == PrincipalType.User && x.PrincipalId == userId);
            return m?.Role;
        }

        public async Task AddMemberAsync(SpaceMember member)
        {
            _db.SpaceMembers.Add(member);
            await _db.SaveChangesAsync();
        }
    }
}

