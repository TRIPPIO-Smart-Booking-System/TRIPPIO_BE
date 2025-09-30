using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Identity;

namespace Trippio.Data
{
    public class DataSeeder
    {
        public async Task SeedAsync(TrippioDbContext context)
        {
            var passwordHasher = new PasswordHasher<AppUser>();

            // Kiểm tra và thêm role "admin" nếu chưa tồn tại
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "ADMIN");
            if (adminRole == null)
            {
                adminRole = new AppRole
                {
                    Id = Guid.Parse("39D2FA36-117C-4552-AC04-7A90993075FF"),
                    Name = "admin",
                    NormalizedName = "ADMIN",
                    DisplayName = "Quản trị viên",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                await context.Roles.AddAsync(adminRole);
                await context.SaveChangesAsync();
            }
            var rootAdminRoleId = adminRole.Id;

            // Helper method tạo user nếu chưa tồn tại
            async Task<AppUser> CreateAdminUserIfNotExists(string userName, string email, string phone)
            {
                var normalizedUserName = userName.ToUpper();
                var user = await context.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName);
                if (user != null) return user;

                var userId = Guid.NewGuid();
                user = new AppUser
                {
                    Id = userId,
                    FirstName = userName, // bạn có thể tách First/Last name nếu muốn
                    LastName = "Admin",
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    UserName = userName,
                    NormalizedUserName = normalizedUserName,
                    PhoneNumber = phone,
                    IsActive = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    LockoutEnabled = false,
                    DateCreated = DateTime.UtcNow,
                    Dob = new DateTime(1990, 1, 1),
                    IsEmailVerified = false,
                    IsFirstLogin = true,
                    Balance = 10000,
                    LoyaltyAmountPerPost = 1000
                };
                user.PasswordHash = passwordHasher.HashPassword(user, "Admin@123$");
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                // Gán role admin cho user
                var userRole = await context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == rootAdminRoleId);
                if (userRole == null)
                {
                    await context.UserRoles.AddAsync(new IdentityUserRole<Guid>
                    {
                        UserId = userId,
                        RoleId = rootAdminRoleId
                    });
                    await context.SaveChangesAsync();
                }

                return user;
            }

            // Tạo 3 tài khoản admin
            await CreateAdminUserIfNotExists("VietAdmin", "vietbmt19@gmail.com", "0977452762");
            await CreateAdminUserIfNotExists("LinhLonton", "linhbinhtinh12344@gmail.com", "0382574698");
            await CreateAdminUserIfNotExists("DuyAnhBulon", "tranhoduyanh03@gmail.com", "091234567");
        }
    }
}
