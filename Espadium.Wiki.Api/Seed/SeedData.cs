using Espadium.Wiki.Infrastructure;
using Espadium.Wiki.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Api.Seed;

public static class SeedData
{
    public static async Task EnsureAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<WikiDbContext>();
        await db.Database.MigrateAsync();

        var um = sp.GetRequiredService<UserManager<User>>();
        var rm = sp.GetRequiredService<RoleManager<Role>>();

        if (!await rm.RoleExistsAsync("SiteAdmin"))
            await rm.CreateAsync(new Role { Id = Guid.NewGuid(), Name = "SiteAdmin" });

        var admin = await um.FindByEmailAsync("admin@local");
        if (admin == null)
        {
            admin = new User { Id = Guid.NewGuid(), Email = "admin@local", UserName = "admin@local", EmailConfirmed = true };
            await um.CreateAsync(admin, "Admin!123");
            await um.AddToRoleAsync(admin, "SiteAdmin");
        }

        if (!await db.Spaces.AnyAsync())
        {
            var spaceId = Guid.NewGuid();
            db.Spaces.Add(new Domain.Entities.Space { Id = spaceId, Key = "DEMO", Name = "Demo Space", CreatedBy = admin.Id, CreatedAt = DateTimeOffset.UtcNow });
            var pageId = Guid.NewGuid();
            db.Pages.Add(new Domain.Entities.Page
            {
                Id = pageId,
                SpaceId = spaceId,
                Slug = "welcome",
                Title = "Welcome",
                Status = Domain.Entities.PageStatus.Published,
                CreatedBy = admin.Id,
                UpdatedBy = admin.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                IsRestricted = false,
                SnapshotJson = "{\"type\":\"doc\",\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}]}"
            });
            await db.SaveChangesAsync();
        }
    }
}

