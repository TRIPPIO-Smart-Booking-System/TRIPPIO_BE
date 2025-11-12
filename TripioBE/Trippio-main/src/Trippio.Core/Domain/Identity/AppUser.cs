using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Trippio.Core.Domain.Entities;

namespace Trippio.Core.Domain.Identity
{
    [Table("AppUsers")]
    public class AppUser : IdentityUser<Guid>
    {
        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }
        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }

        // Override PhoneNumber to make it required and add validation
        [Required]
        [Phone]
        [MaxLength(15)]
        public override string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime Dob { get; set; }
        [MaxLength(500)]
        public string? Avatar { get; set; }
        public DateTime? VipStartDate { get; set; }
        public DateTime? VipExpireDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public double Balance { get; set; }
        public double LoyaltyAmountPerPost { get; set; }  // Default value, can be changed later

        // Email verification fields
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }
        public string? EmailOtp { get; set; }
        public DateTime? EmailOtpExpiry { get; set; }

        // Password reset fields
        public string? PasswordResetOtp { get; set; }
        public DateTime? PasswordResetOtpExpiry { get; set; }

        public bool IsFirstLogin { get; set; } = true;

        // 🔐 Google OAuth Fields
        [MaxLength(255)]
        public string? GoogleId { get; set; }
        [MaxLength(500)]
        public string? Picture { get; set; }
        [MaxLength(50)]
        public string OAuthProvider { get; set; } = "google";

        // Navigation Properties
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        public string? GetFullName() => $"{FirstName} {LastName}";
    }
}
