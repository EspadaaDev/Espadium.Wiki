using Espadium.Wiki.Application.DTOs;
using Espadium.Wiki.Domain.Entities;
using Espadium.Wiki.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Application.Services
{
    public class PageService
    {
        private readonly WikiDbContext _context;

        public PageService(WikiDbContext context)
        {
            _context = context;
        }

        public async Task<List<PageDto>> GetPagesAsync(Guid? spaceId = null)
        {
            var query = _context.Pages.AsQueryable();
            if (spaceId.HasValue)
                query = query.Where(p => p.SpaceId == spaceId.Value);

            return await query
                .Select(p => new PageDto(p.Id, p.SpaceId, p.ParentId, p.Slug, p.Title, p.Status, p.CreatedBy, p.UpdatedBy, p.CreatedAt, p.UpdatedAt))
                .ToListAsync();
        }

        public async Task<PageDto?> GetPageAsync(Guid id)
        {
            var page = await _context.Pages.FindAsync(id);
            return page == null ? null : new PageDto(page.Id, page.SpaceId, page.ParentId, page.Slug, page.Title, page.Status, page.CreatedBy, page.UpdatedBy, page.CreatedAt, page.UpdatedAt);
        }

        public async Task<PageDto> CreatePageAsync(CreatePageRequest request, Guid userId)
        {
            var page = new Page
            {
                Id = Guid.NewGuid(),
                SpaceId = request.SpaceId,
                ParentId = request.ParentId,
                Slug = request.Slug,
                Title = request.Title,
                Status = request.Status,
                CreatedBy = userId,
                UpdatedBy = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Pages.Add(page);
            await _context.SaveChangesAsync();

            return new PageDto(page.Id, page.SpaceId, page.ParentId, page.Slug, page.Title, page.Status, page.CreatedBy, page.UpdatedBy, page.CreatedAt, page.UpdatedAt);
        }

        public async Task<PageDto?> UpdatePageAsync(Guid id, UpdatePageRequest request, Guid userId)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page == null) return null;

            page.Title = request.Title;
            page.Status = request.Status;
            page.UpdatedBy = userId;
            page.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return new PageDto(page.Id, page.SpaceId, page.ParentId, page.Slug, page.Title, page.Status, page.CreatedBy, page.UpdatedBy, page.CreatedAt, page.UpdatedAt);
        }

        public async Task<bool> DeletePageAsync(Guid id)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page == null) return false;

            _context.Pages.Remove(page);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
