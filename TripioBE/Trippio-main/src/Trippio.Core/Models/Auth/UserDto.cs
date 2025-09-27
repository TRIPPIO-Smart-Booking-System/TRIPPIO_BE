namespace Trippio.Core.Models.Auth
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Avatar { get; set; }
        public double Balance { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public List<string> Roles { get; set; } = new();
    }
}