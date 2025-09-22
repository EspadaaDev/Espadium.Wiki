using Espadium.Wiki.Application.Abstractions.Repositories;
using Espadium.Wiki.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Infrastructure.Repositories
{
    public class EfTeamRepository : ITeamRepository
    {
        private readonly WikiDbContext _db;
        public EfTeamRepository(WikiDbContext db) { _db = db; }

        public async Task AddAsync(Team team)
        {
            _db.Teams.Add(team);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Team team)
        {
            _db.Teams.Remove(team);
            await _db.SaveChangesAsync();
        }

        public Task<Team?> GetAsync(Guid id) => _db.Teams.FindAsync(id).AsTask();

        public Task<List<Team>> ListAsync() => _db.Teams.AsNoTracking().ToListAsync();

        public async Task UpdateAsync(Team team)
        {
            _db.Teams.Update(team);
            await _db.SaveChangesAsync();
        }
    }
}

