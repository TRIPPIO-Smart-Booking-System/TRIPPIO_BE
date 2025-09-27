using Trippio.Data;
using Microsoft.EntityFrameworkCore;

namespace Trippio.Api
{
    public static class MigrationManager
    {
        public static WebApplication MigrateDatabase(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<TrippioDbContext>())
                {
                    context.Database.Migrate();
                    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                    dataSeeder.SeedAsync(context).Wait();
                }
            }
            return app;
        }
    }
}