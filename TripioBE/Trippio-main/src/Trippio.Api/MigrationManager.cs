using Trippio.Data;
using Trippio.Core.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Trippio.Api
{
    public static class MigrationManager
    {
        public static async Task<WebApplication> MigrateDatabaseAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<TrippioDbContext>())
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    
                    // Retry logic for database connection
                    var retries = 10;
                    var delay = TimeSpan.FromSeconds(5);
                    
                    for (int i = 0; i < retries; i++)
                    {
                        try
                        {
                            logger.LogInformation("Attempting to migrate database... Attempt {Attempt}/{MaxAttempts}", i + 1, retries);
                            
                            // Try to migrate directly - EF will create database if needed
                            logger.LogInformation("Running database migrations...");
                            await context.Database.MigrateAsync();
                            
                            logger.LogInformation("Database migration completed. Running data seeder...");
                            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
                            var dataSeeder = new DataSeeder(userManager, roleManager);
                            await dataSeeder.SeedAsync(context);
                            
                            logger.LogInformation("Data seeding completed successfully.");
                            break;
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Database migration attempt {Attempt} failed: {Message}", i + 1, ex.Message);
                            
                            if (i == retries - 1)
                            {
                                logger.LogError(ex, "Failed to migrate database after {MaxAttempts} attempts", retries);
                                throw;
                            }
                            
                            logger.LogInformation("Waiting {Delay} seconds before retry...", delay.TotalSeconds);
                            await Task.Delay(delay);
                        }
                    }
                }
            }
            return app;
        }
    }
}