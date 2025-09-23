using Espadium.Wiki.Domain.Entities;
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
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
        public DbSet<Space> Spaces => Set<Space>();
        public DbSet<SpaceMember> SpaceMembers => Set<SpaceMember>();
        public DbSet<Page> Pages => Set<Page>();
        public DbSet<PageRevision> PageRevisions => Set<PageRevision>();
        public DbSet<Attachment> Attachments => Set<Attachment>();
        public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
        public DbSet<AdminSettingsAudit> AdminSettingsAudits => Set<AdminSettingsAudit>();
        public DbSet<PublicLink> PublicLinks => Set<PublicLink>();

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

            modelBuilder.Entity<Team>(b =>
            {
                b.HasKey(t => t.Id);
                b.Property(t => t.Name).IsRequired().HasMaxLength(200);
                b.Property(t => t.Description).HasMaxLength(1000);
                b.HasMany(t => t.Members).WithOne(tm => tm.Team).HasForeignKey(tm => tm.TeamId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TeamMember>(b =>
            {
                b.HasKey(tm => new { tm.TeamId, tm.UserId });
                b.Property(tm => tm.Role).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<Space>(b =>
            {
                b.HasKey(s => s.Id);
                b.Property(s => s.Key).IsRequired().HasMaxLength(100);
                b.Property(s => s.Name).IsRequired().HasMaxLength(200);
                b.Property(s => s.Description).HasMaxLength(1000);
                b.HasIndex(s => s.Key).IsUnique();
                b.HasMany(s => s.Members).WithOne(sm => sm.Space).HasForeignKey(sm => sm.SpaceId).OnDelete(DeleteBehavior.Cascade);
                b.HasMany(s => s.Pages).WithOne(p => p.Space).HasForeignKey(p => p.SpaceId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SpaceMember>(b =>
            {
                b.HasKey(sm => new { sm.SpaceId, sm.PrincipalType, sm.PrincipalId });
                b.Property(sm => sm.PrincipalType).HasConversion<string>();
                b.Property(sm => sm.Role).HasConversion<string>();
            });

            modelBuilder.Entity<Page>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Slug).IsRequired().HasMaxLength(200);
                b.Property(p => p.Title).IsRequired().HasMaxLength(500);
                b.Property(p => p.Status).HasConversion<string>();
                b.Property(p => p.IsRestricted).HasDefaultValue(false);
                b.HasIndex(p => new { p.SpaceId, p.Slug }).IsUnique();
                b.HasOne(p => p.Parent).WithMany(p => p.Children).HasForeignKey(p => p.ParentId).OnDelete(DeleteBehavior.Restrict);
                b.HasMany(p => p.Revisions).WithOne(pr => pr.Page).HasForeignKey(pr => pr.PageId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PageRevision>(b =>
            {
                b.HasKey(pr => pr.Id);
                b.Property(pr => pr.SnapshotJson).IsRequired();
                b.HasIndex(pr => new { pr.PageId, pr.RevisionNo }).IsUnique();
            });

            modelBuilder.Entity<PageRestriction>(b =>
            {
                b.HasKey(r => new { r.PageId, r.UserId });
                b.ToTable("page_restrictions");
                b.HasOne(r => r.Page).WithMany().HasForeignKey(r => r.PageId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Attachment>(b =>
            {
                b.HasKey(a => a.Id);
                b.Property(a => a.StorageKey).IsRequired().HasMaxLength(512);
                b.Property(a => a.Bucket).IsRequired().HasMaxLength(128);
                b.Property(a => a.Filename).IsRequired().HasMaxLength(256);
                b.Property(a => a.Mime).IsRequired().HasMaxLength(128);
                b.Property(a => a.Status).IsRequired().HasMaxLength(32);
                b.Property(a => a.AvScan).IsRequired().HasMaxLength(32);
                b.HasIndex(a => a.PageId);
                b.HasIndex(a => a.StorageKey).IsUnique();
                b.HasOne(a => a.Page).WithMany(p => p.Attachments).HasForeignKey(a => a.PageId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SystemSetting>(b =>
            {
                b.HasKey(s => s.Key);
                b.Property(s => s.Key).HasMaxLength(150);
                b.Property(s => s.Value).IsRequired();
                b.HasIndex(s => s.UpdatedAt);
            });

            modelBuilder.Entity<AdminSettingsAudit>(b =>
            {
                b.HasKey(a => a.Id);
                b.Property(a => a.Key).HasMaxLength(150);
                b.HasIndex(a => a.ChangedAt);
            });

            modelBuilder.Entity<PublicLink>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Token).IsRequired().HasMaxLength(200);
                b.HasIndex(p => p.Token).IsUnique();
                b.HasIndex(p => p.PageId);
            });
        }
    }
}

