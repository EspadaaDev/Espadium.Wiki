using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Espadium.Wiki.Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WikiDbContext>
    {
        public WikiDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WikiDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=wiki;Username=postgres;Password=postgres");
            return new WikiDbContext(optionsBuilder.Options);
        }
    }
}

