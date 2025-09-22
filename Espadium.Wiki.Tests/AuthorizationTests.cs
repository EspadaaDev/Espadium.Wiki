using Espadium.Wiki.Application.Services;
using Espadium.Wiki.Domain.Entities;
using Espadium.Wiki.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Tests
{
    public class AuthorizationTests
    {
        private static WikiDbContext NewInMemory()
        {
            var options = new DbContextOptionsBuilder<WikiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new WikiDbContext(options);
        }

        [Fact]
        public async Task Viewer_has_view_but_not_edit()
        {
            using var db = NewInMemory();
            var spaceId = Guid.NewGuid(); var userId = Guid.NewGuid();
            db.Spaces.Add(new Space { Id = spaceId, Key = "ENG", Name = "Eng", CreatedBy = userId, CreatedAt = DateTimeOffset.UtcNow });
            db.SpaceMembers.Add(new SpaceMember { SpaceId = spaceId, PrincipalType = PrincipalType.User, PrincipalId = userId, Role = SpaceRole.Viewer });
            var pageId = Guid.NewGuid();
            db.Pages.Add(new Page { Id = pageId, SpaceId = spaceId, Slug = "home", Title = "Home", Status = PageStatus.Published, CreatedBy = userId, UpdatedBy = userId, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();

            var svc = new PermissionService(db);
            (await svc.HasSpaceRoleAsync(userId, spaceId, SpaceRole.Viewer)).Should().BeTrue();
            (await svc.CanEditPageAsync(userId, pageId)).Should().BeFalse();
        }

        [Fact]
        public async Task Contributor_can_edit_unrestricted()
        {
            using var db = NewInMemory();
            var spaceId = Guid.NewGuid(); var userId = Guid.NewGuid();
            db.Spaces.Add(new Space { Id = spaceId, Key = "ENG", Name = "Eng", CreatedBy = userId, CreatedAt = DateTimeOffset.UtcNow });
            db.SpaceMembers.Add(new SpaceMember { SpaceId = spaceId, PrincipalType = PrincipalType.User, PrincipalId = userId, Role = SpaceRole.Contributor });
            var pageId = Guid.NewGuid();
            db.Pages.Add(new Page { Id = pageId, SpaceId = spaceId, Slug = "home", Title = "Home", Status = PageStatus.Published, CreatedBy = userId, UpdatedBy = userId, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();

            var svc = new PermissionService(db);
            (await svc.CanEditPageAsync(userId, pageId)).Should().BeTrue();
        }

        [Fact]
        public async Task Restricted_page_requires_explicit_entry()
        {
            using var db = NewInMemory();
            var spaceId = Guid.NewGuid(); var userId = Guid.NewGuid();
            db.Spaces.Add(new Space { Id = spaceId, Key = "ENG", Name = "Eng", CreatedBy = userId, CreatedAt = DateTimeOffset.UtcNow });
            db.SpaceMembers.Add(new SpaceMember { SpaceId = spaceId, PrincipalType = PrincipalType.User, PrincipalId = userId, Role = SpaceRole.Admin });
            var pageId = Guid.NewGuid();
            db.Pages.Add(new Page { Id = pageId, SpaceId = spaceId, Slug = "home", Title = "Home", Status = PageStatus.Published, IsRestricted = true, CreatedBy = userId, UpdatedBy = userId, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();

            var svc = new PermissionService(db);
            (await svc.CanEditPageAsync(userId, pageId)).Should().BeFalse();

            db.Set<PageRestriction>().Add(new PageRestriction { PageId = pageId, UserId = userId, CanEdit = true });
            await db.SaveChangesAsync();
            (await svc.CanEditPageAsync(userId, pageId)).Should().BeTrue();
        }

        [Fact]
        public async Task Missing_membership_denies()
        {
            using var db = NewInMemory();
            var spaceId = Guid.NewGuid(); var userId = Guid.NewGuid();
            db.Spaces.Add(new Space { Id = spaceId, Key = "ENG", Name = "Eng", CreatedBy = userId, CreatedAt = DateTimeOffset.UtcNow });
            var pageId = Guid.NewGuid();
            db.Pages.Add(new Page { Id = pageId, SpaceId = spaceId, Slug = "home", Title = "Home", Status = PageStatus.Published, CreatedBy = userId, UpdatedBy = userId, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();

            var svc = new PermissionService(db);
            (await svc.HasSpaceRoleAsync(userId, spaceId, SpaceRole.Viewer)).Should().BeFalse();
            (await svc.CanEditPageAsync(userId, pageId)).Should().BeFalse();
        }
    }
}

