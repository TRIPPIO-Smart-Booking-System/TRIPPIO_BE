using System.ComponentModel.DataAnnotations;

namespace Trippio.Core.Models.Auth
{
    public class VerifyPhoneRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = string.Empty;
    }
}