using Espadium.Wiki.Application.Abstractions;
using Espadium.Wiki.Application.Abstractions.Repositories;
using Espadium.Wiki.Application.DTOs;
using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Application.Services
{
    public class PageService
    {
        private readonly IPageRepository _pages;
        private readonly IDateTimeProvider _clock;

        public PageService(IPageRepository pages, IDateTimeProvider clock)
        {
            _pages = pages;
            _clock = clock;
        }

        public async Task<List<PageDto>> GetPagesAsync(Guid? spaceId = null)
        {
            var list = await _pages.ListAsync(spaceId);
            return list.Select(p => new PageDto(p.Id, p.SpaceId, p.ParentId, p.Slug, p.Title, p.Status, p.CreatedBy, p.UpdatedBy, p.CreatedAt, p.UpdatedAt)).ToList();
        }

        public async Task<PageDto?> GetPageAsync(Guid id)
        {
            var page = await _pages.GetAsync(id);
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
                CreatedAt = _clock.UtcNow,
                UpdatedAt = _clock.UtcNow
            };
            await _pages.AddAsync(page);

            return new PageDto(page.Id, page.SpaceId, page.ParentId, page.Slug, page.Title, page.Status, page.CreatedBy, page.UpdatedBy, page.CreatedAt, page.UpdatedAt);
        }

        public async Task<PageDto?> UpdatePageAsync(Guid id, UpdatePageRequest request, Guid userId)
        {
            var page = await _pages.GetAsync(id);
            if (page == null) return null;

            page.Title = request.Title;
            page.Status = request.Status;
            page.UpdatedBy = userId;
            page.UpdatedAt = _clock.UtcNow;
            await _pages.UpdateAsync(page);
            return new PageDto(page.Id, page.SpaceId, page.ParentId, page.Slug, page.Title, page.Status, page.CreatedBy, page.UpdatedBy, page.CreatedAt, page.UpdatedAt);
        }

        public async Task<bool> DeletePageAsync(Guid id)
        {
            var page = await _pages.GetAsync(id);
            if (page == null) return false;
            await _pages.DeleteAsync(page);
            return true;
        }
    }
}
