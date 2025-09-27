namespace Trippio.Core.Models.System
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public bool IsActive { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public DateTime Dob { get; set; }
        public string? Avatar { get; set; }
        public DateTime? VipStartDate { get; set; }
        public DateTime? VipExpireDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public double Balance { get; set; }
        public double LoyaltyAmountPerPost { get; set; }
    }
}
