using Espadium.Wiki.Application.DTOs;
using Espadium.Wiki.Domain.Entities;
using Espadium.Wiki.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Application.Services
{
    public class SpaceService
    {
        private readonly WikiDbContext _context;

        public SpaceService(WikiDbContext context)
        {
            _context = context;
        }

        public async Task<List<SpaceDto>> GetSpacesAsync()
        {
            return await _context.Spaces
                .Select(s => new SpaceDto(s.Id, s.Key, s.Name, s.Description, s.CreatedBy, s.IsPrivate, s.CreatedAt))
                .ToListAsync();
        }

        public async Task<SpaceDto?> GetSpaceAsync(Guid id)
        {
            var space = await _context.Spaces.FindAsync(id);
            return space == null ? null : new SpaceDto(space.Id, space.Key, space.Name, space.Description, space.CreatedBy, space.IsPrivate, space.CreatedAt);
        }

        public async Task<SpaceDto> CreateSpaceAsync(CreateSpaceRequest request, Guid userId)
        {
            var space = new Space
            {
                Id = Guid.NewGuid(),
                Key = request.Key,
                Name = request.Name,
                Description = request.Description,
                IsPrivate = request.IsPrivate,
                CreatedBy = userId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Spaces.Add(space);
            await _context.SaveChangesAsync();

            return new SpaceDto(space.Id, space.Key, space.Name, space.Description, space.CreatedBy, space.IsPrivate, space.CreatedAt);
        }

        public async Task<SpaceDto?> UpdateSpaceAsync(Guid id, UpdateSpaceRequest request)
        {
            var space = await _context.Spaces.FindAsync(id);
            if (space == null) return null;

            space.Name = request.Name;
            space.Description = request.Description;
            space.IsPrivate = request.IsPrivate;

            await _context.SaveChangesAsync();
            return new SpaceDto(space.Id, space.Key, space.Name, space.Description, space.CreatedBy, space.IsPrivate, space.CreatedAt);
        }

        public async Task<bool> DeleteSpaceAsync(Guid id)
        {
            var space = await _context.Spaces.FindAsync(id);
            if (space == null) return false;

            _context.Spaces.Remove(space);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddMemberAsync(Guid spaceId, AddSpaceMemberRequest request)
        {
            var exists = await _context.SpaceMembers
                .AnyAsync(sm => sm.SpaceId == spaceId && sm.PrincipalType == request.PrincipalType && sm.PrincipalId == request.PrincipalId);
            
            if (exists) return false;

            var member = new SpaceMember
            {
                SpaceId = spaceId,
                PrincipalType = request.PrincipalType,
                PrincipalId = request.PrincipalId,
                Role = request.Role
            };

            _context.SpaceMembers.Add(member);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
