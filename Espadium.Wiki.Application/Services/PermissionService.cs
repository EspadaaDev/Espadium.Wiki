using Espadium.Wiki.Domain.Entities;
using Espadium.Wiki.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Application.Services
{
    public interface IPermissionService
    {
        Task<bool> HasSpaceRoleAsync(Guid userId, Guid spaceId, SpaceRole minRole);
        Task<bool> CanEditPageAsync(Guid userId, Guid pageId);
    }

    public class PermissionService : IPermissionService
    {
        private readonly WikiDbContext _db;

        public PermissionService(WikiDbContext db)
        {
            _db = db;
        }

        public async Task<bool> HasSpaceRoleAsync(Guid userId, Guid spaceId, SpaceRole minRole)
        {
            var m = await _db.SpaceMembers.FirstOrDefaultAsync(x => x.SpaceId == spaceId && x.PrincipalType == PrincipalType.User && x.PrincipalId == userId);
            if (m == null) return false;
            return RoleRank(m.Role) >= RoleRank(minRole);
        }

        public async Task<bool> CanEditPageAsync(Guid userId, Guid pageId)
        {
            var page = await _db.Pages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pageId);
            if (page == null) return false;
            var hasRole = await HasSpaceRoleAsync(userId, page.SpaceId, SpaceRole.Contributor);
            if (!page.IsRestricted) return hasRole;
            if (!hasRole) return false;
            var pr = await _db.Set<PageRestriction>().FirstOrDefaultAsync(r => r.PageId == pageId && r.UserId == userId);
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

