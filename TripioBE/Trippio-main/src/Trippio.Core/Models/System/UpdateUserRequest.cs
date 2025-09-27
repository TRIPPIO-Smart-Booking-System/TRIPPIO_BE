namespace Trippio.Core.Models.System
{
    public class UpdateUserRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public DateTime? Dob { get; set; }
        public string? Avatar { get; set; }
    }
}
