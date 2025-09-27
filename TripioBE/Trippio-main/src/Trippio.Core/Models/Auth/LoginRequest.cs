using System.ComponentModel.DataAnnotations;

namespace Trippio.Core.Models.Auth
{
    public class LoginRequest
    {
        [Required]
        [StringLength(100)]
        public string UsernameOrPhone { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
