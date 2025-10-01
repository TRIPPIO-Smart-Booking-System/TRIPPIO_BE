using Trippio.Core.Domain.Identity;
using Trippio.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Trippio.Data
{
    public class TrippioDbContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public TrippioDbContext(DbContextOptions<TrippioDbContext> options) : base(options)
        {
        }

        // Customer & Address
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }

        // Chat & Notification
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Order
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // Booking
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<AccommodationBookingDetail> AccommodationBookingDetails { get; set; }
        public DbSet<TransportBookingDetail> TransportBookingDetails { get; set; }
        public DbSet<EntertainmentBookingDetail> EntertainmentBookingDetails { get; set; }
        public DbSet<ExtraService> ExtraServices { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Comment> Comments { get; set; }

        // Payment & Others
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ScheduledJob> ScheduledJobs { get; set; }
        public DbSet<Basket> Baskets { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Identity tables configuration
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("AppUserClaims")
           .HasKey(x => x.Id);

            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AppRoleClaims")
                   .HasKey(x => x.Id);

            builder.Entity<IdentityUserRole<Guid>>().ToTable("AppUserRoles")
                   .HasKey(x => new { x.RoleId, x.UserId });

            builder.Entity<IdentityUserLogin<Guid>>().ToTable("AppUserLogins")
                   .HasKey(x => new { x.UserId, x.LoginProvider });

            builder.Entity<IdentityUserToken<Guid>>().ToTable("AppUserTokens")
                   .HasKey(x => new { x.UserId, x.LoginProvider, x.Name });

            // Customer & Address relationships
            builder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Address>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Addresses)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Conversation & ChatMessage relationships
            builder.Entity<Conversation>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Conversation>()
                .HasOne(c => c.Coach)
                .WithMany()
                .HasForeignKey(c => c.CoachId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatMessage>()
                .HasOne(cm => cm.Conversation)
                .WithMany(c => c.ChatMessages)
                .HasForeignKey(cm => cm.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChatMessage>()
                .HasOne(cm => cm.SenderUser)
                .WithMany()
                .HasForeignKey(cm => cm.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatMessage>()
                .HasOne(cm => cm.ReceiverUser)
                .WithMany()
                .HasForeignKey(cm => cm.ReceiverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification relationship
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order relationships
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Booking)
                .WithMany()
                .HasForeignKey(oi => oi.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking relationships
            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AccommodationBookingDetail>()
                .HasOne(abd => abd.Booking)
                .WithMany(b => b.AccommodationBookingDetails)
                .HasForeignKey(abd => abd.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TransportBookingDetail>()
                .HasOne(tbd => tbd.Booking)
                .WithMany(b => b.TransportBookingDetails)
                .HasForeignKey(tbd => tbd.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EntertainmentBookingDetail>()
                .HasOne(ebd => ebd.Booking)
                .WithMany(b => b.EntertainmentBookingDetails)
                .HasForeignKey(ebd => ebd.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment relationships
            builder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Basket relationship
            builder.Entity<Basket>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ExtraService relationship
            builder.Entity<ExtraService>()
                .HasOne(es => es.Booking)
                .WithMany(b => b.ExtraServices)
                .HasForeignKey(es => es.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Feedback relationship
            builder.Entity<Feedback>()
                .HasOne(f => f.Booking)
                .WithMany(b => b.Feedbacks)
                .HasForeignKey(f => f.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment relationship
            builder.Entity<Comment>()
                .HasOne(c => c.Booking)
                .WithMany(b => b.Comments)
                .HasForeignKey(c => c.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

        }
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified).ToList();

            foreach (var entityEntry in entries)
            {
                var dateCreatedProp = entityEntry.Entity.GetType().GetProperty("DateCreated");
                var createdAtProp = entityEntry.Entity.GetType().GetProperty("CreatedAt");
                
                if (entityEntry.State == EntityState.Added)
                {
                    if (dateCreatedProp != null)
                    {
                        dateCreatedProp.SetValue(entityEntry.Entity, DateTime.UtcNow);
                    }
                    if (createdAtProp != null)
                    {
                        createdAtProp.SetValue(entityEntry.Entity, DateTime.UtcNow);
                    }
                }

                var modifiedDateProp = entityEntry.Entity.GetType().GetProperty("ModifiedDate");
                if (entityEntry.State == EntityState.Modified)
                {
                    if (modifiedDateProp != null)
                    {
                        modifiedDateProp.SetValue(entityEntry.Entity, DateTime.UtcNow);
                    }
                }
            }

            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}
