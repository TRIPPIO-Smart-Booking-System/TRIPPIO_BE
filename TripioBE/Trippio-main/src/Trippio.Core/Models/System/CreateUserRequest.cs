namespace Trippio.Core.Models.System
{
    public class CreateUserRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string UserName { get; set; }
        public string? Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Password { get; set; }

        public bool IsActive { get; set; }
        public string? Avatar { get; set; }
    }
}
