using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Application.Abstractions
{
    public interface IPermissionService
    {
        Task<bool> HasSpaceRoleAsync(Guid userId, Guid spaceId, SpaceRole minRole);
        Task<bool> CanEditPageAsync(Guid userId, Guid pageId);
    }
}

