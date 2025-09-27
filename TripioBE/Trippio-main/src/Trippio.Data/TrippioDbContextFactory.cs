using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Trippio.Data
{
    public class TrippioDbContextFactory : IDesignTimeDbContextFactory<TrippioDbContext>
    {
        public TrippioDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var builder = new DbContextOptionsBuilder<TrippioDbContext>();
            builder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            return new TrippioDbContext(builder.Options);
        }
    }
}
