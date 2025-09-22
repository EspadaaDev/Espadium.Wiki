using Espadium.Wiki.Application.Abstractions;
using Espadium.Wiki.Application.Abstractions.Repositories;
using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ISpaceRepository _spaces;
        private readonly IPageRepository _pages;
        private readonly IPageRestrictionRepository _restrictions;

        public PermissionService(ISpaceRepository spaces, IPageRepository pages, IPageRestrictionRepository restrictions)
        {
            _spaces = spaces;
            _pages = pages;
            _restrictions = restrictions;
        }

        public async Task<bool> HasSpaceRoleAsync(Guid userId, Guid spaceId, SpaceRole minRole)
        {
            var role = await _spaces.GetUserRoleAsync(spaceId, userId);
            if (role is null) return false;
            return RoleRank(role.Value) >= RoleRank(minRole);
        }

        public async Task<bool> CanEditPageAsync(Guid userId, Guid pageId)
        {
            var page = await _pages.GetAsync(pageId);
            if (page == null) return false;
            var hasRole = await HasSpaceRoleAsync(userId, page.SpaceId, SpaceRole.Contributor);
            if (!page.IsRestricted) return hasRole;
            if (!hasRole) return false;
            var pr = await _restrictions.GetAsync(pageId, userId);
            return pr?.CanEdit == true;
        }

        private static int RoleRank(SpaceRole role) => role switch
        {
            SpaceRole.Viewer => 0,
            SpaceRole.Contributor => 1,
            SpaceRole.Admin => 2,
            _ => -1
        };
    }
}

