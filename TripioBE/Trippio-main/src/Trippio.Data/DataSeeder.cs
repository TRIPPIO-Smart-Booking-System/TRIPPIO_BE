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
                    Id = Guid.Parse("39D2FA36-117C-4552-AC04-7A90993075FF"), // Sử dụng ID cố định
                    Name = "admin",
                    NormalizedName = "ADMIN",
                    DisplayName = "Quản trị viên",
                    ConcurrencyStamp = Guid.NewGuid().ToString() // Tạo mới nếu cần
                };
                await context.Roles.AddAsync(adminRole);
                await context.SaveChangesAsync();
            }
            var rootAdminRoleId = adminRole.Id; // Lấy ID của role hiện có hoặc vừa tạo

            // Kiểm tra và thêm user "VietAdmin" nếu chưa tồn tại
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == "VIETADMIN");
            if (adminUser == null)
            {
                var userId = Guid.NewGuid();
                adminUser = new AppUser
                {
                    Id = userId,
                    FirstName = "Viet",
                    LastName = "Admin",
                    Email = "vietbmt19@gmail.com",
                    NormalizedEmail = "VIETBMT19@GMAIL.COM",
                    UserName = "VietAdmin",
                    NormalizedUserName = "VIETADMIN",
                    PhoneNumber = "0977452762",
                    IsActive = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    LockoutEnabled = false,
                    DateCreated = DateTime.UtcNow,
                    Dob = new DateTime(1990, 1, 1),
                    IsEmailVerified = true,
                    IsPhoneVerified = true,
                    IsFirstLogin = false,
                    Balance = 10000,
                    LoyaltyAmountPerPost = 1000
                };
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123$");
                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();

                // Gán role cho user
                var userRole = await context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == rootAdminRoleId);
                if (userRole == null)
                {
                    await context.UserRoles.AddAsync(new IdentityUserRole<Guid>
                    {
                        RoleId = rootAdminRoleId,
                        UserId = userId,
                    });
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}