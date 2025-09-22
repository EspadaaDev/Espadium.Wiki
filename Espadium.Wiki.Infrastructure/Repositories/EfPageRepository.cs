using Espadium.Wiki.Application.Abstractions.Repositories;
using Espadium.Wiki.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Infrastructure.Repositories
{
    public class EfPageRepository : IPageRepository
    {
        private readonly WikiDbContext _db;
        public EfPageRepository(WikiDbContext db) { _db = db; }

        public async Task AddAsync(Page page)
        {
            _db.Pages.Add(page);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Page page)
        {
            _db.Pages.Remove(page);
            await _db.SaveChangesAsync();
        }

        public Task<Page?> GetAsync(Guid id) => _db.Pages.FindAsync(id).AsTask();

        public Task<List<Page>> ListAsync(Guid? spaceId = null)
        {
            var q = _db.Pages.AsNoTracking().AsQueryable();
            if (spaceId.HasValue) q = q.Where(p => p.SpaceId == spaceId.Value);
            return q.ToListAsync();
        }

        public async Task UpdateAsync(Page page)
        {
            _db.Pages.Update(page);
            await _db.SaveChangesAsync();
        }
    }
}

