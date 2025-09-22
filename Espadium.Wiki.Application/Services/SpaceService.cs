using Espadium.Wiki.Application.Abstractions;
using Espadium.Wiki.Application.Abstractions.Repositories;
using Espadium.Wiki.Application.DTOs;
using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Application.Services
{
    public class SpaceService
    {
        private readonly ISpaceRepository _spaces;
        private readonly IDateTimeProvider _clock;

        public SpaceService(ISpaceRepository spaces, IDateTimeProvider clock)
        {
            _spaces = spaces;
            _clock = clock;
        }

        public async Task<List<SpaceDto>> GetSpacesAsync()
        {
            var list = await _spaces.ListAsync();
            return list.Select(s => new SpaceDto(s.Id, s.Key, s.Name, s.Description, s.CreatedBy, s.IsPrivate, s.CreatedAt)).ToList();
        }

        public async Task<SpaceDto?> GetSpaceAsync(Guid id)
        {
            var space = await _spaces.GetAsync(id);
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
                CreatedAt = _clock.UtcNow
            };
            await _spaces.AddAsync(space);

            return new SpaceDto(space.Id, space.Key, space.Name, space.Description, space.CreatedBy, space.IsPrivate, space.CreatedAt);
        }

        public async Task<SpaceDto?> UpdateSpaceAsync(Guid id, UpdateSpaceRequest request)
        {
            var space = await _spaces.GetAsync(id);
            if (space == null) return null;

            space.Name = request.Name;
            space.Description = request.Description;
            space.IsPrivate = request.IsPrivate;
            await _spaces.UpdateAsync(space);
            return new SpaceDto(space.Id, space.Key, space.Name, space.Description, space.CreatedBy, space.IsPrivate, space.CreatedAt);
        }

        public async Task<bool> DeleteSpaceAsync(Guid id)
        {
            var space = await _spaces.GetAsync(id);
            if (space == null) return false;
            await _spaces.DeleteAsync(space);
            return true;
        }

        public async Task<bool> AddMemberAsync(Guid spaceId, AddSpaceMemberRequest request)
        {
            if (await _spaces.ExistsMemberAsync(spaceId, request.PrincipalType, request.PrincipalId)) return false;
            await _spaces.AddMemberAsync(new SpaceMember
            {
                SpaceId = spaceId,
                PrincipalType = request.PrincipalType,
                PrincipalId = request.PrincipalId,
                Role = request.Role
            });
            return true;
        }
    }
}
