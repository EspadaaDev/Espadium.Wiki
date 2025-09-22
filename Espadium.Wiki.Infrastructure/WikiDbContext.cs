using Espadium.Wiki.Infrastructure.Identity;
using Espadium.Wiki.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Infrastructure
{
    public class WikiDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public WikiDbContext(DbContextOptions<WikiDbContext> options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("AspNetUsers", schema: "auth");
            modelBuilder.Entity<Role>().ToTable("AspNetRoles", schema: "auth");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims", schema: "auth");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("AspNetUserRoles", schema: "auth");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins", schema: "auth");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("AspNetRoleClaims", schema: "auth");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("AspNetUserTokens", schema: "auth");

            modelBuilder.Entity<RefreshToken>(b =>
            {
                b.ToTable("refresh_tokens", schema: "auth");
                b.HasKey(rt => rt.Id);
                b.Property(rt => rt.Token).IsRequired();
                b.HasIndex(rt => rt.Token).IsUnique();
                b.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

