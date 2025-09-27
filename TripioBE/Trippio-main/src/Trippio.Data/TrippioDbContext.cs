using Trippio.Core.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Trippio.Data
{
    public class TrippioDbContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public TrippioDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("AppUserClaims")
           .HasKey(x => x.Id);

            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AppRoleClaims")
                   .HasKey(x => x.Id);

            builder.Entity<IdentityUserRole<Guid>>().ToTable("AppUserRoles")
                   .HasKey(x => new { x.RoleId, x.UserId });

            builder.Entity<IdentityUserLogin<Guid>>().ToTable("AppUserLogins")
                   .HasKey(x => new { x.UserId });
            builder.Entity<IdentityUserToken<Guid>>().HasNoKey();

        }
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified).ToList();

            foreach (var entityEntry in entries)
            {
                var dateCreatedProp = entityEntry.Entity.GetType().GetProperty("DateCreated");
                if (entityEntry.State == EntityState.Added)
                {
                    if (dateCreatedProp != null)
                    {
                        dateCreatedProp.SetValue(entityEntry.Entity, DateTime.UtcNow);
                    }
                }

                var modifiedDateProp = entityEntry.Entity.GetType().GetProperty("ModifiedDate");
                if (entityEntry.State == EntityState.Modified)
                {
                    if (modifiedDateProp != null)
                    {
                        modifiedDateProp.SetValue(entityEntry.Entity, DateTime.Now);
                    }
                }
            }

            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}
