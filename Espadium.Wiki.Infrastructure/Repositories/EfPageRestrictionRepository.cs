using Espadium.Wiki.Application.Abstractions.Repositories;
using Espadium.Wiki.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Infrastructure.Repositories
{
    public class EfPageRestrictionRepository : IPageRestrictionRepository
    {
        private readonly WikiDbContext _db;
        public EfPageRestrictionRepository(WikiDbContext db) { _db = db; }

        public async Task AddAsync(PageRestriction restriction)
        {
            _db.Set<PageRestriction>().Add(restriction);
            await _db.SaveChangesAsync();
        }

        public Task<PageRestriction?> GetAsync(Guid pageId, Guid userId)
            => _db.Set<PageRestriction>().AsNoTracking().FirstOrDefaultAsync(r => r.PageId == pageId && r.UserId == userId);
    }
}

