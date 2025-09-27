using System.ComponentModel.DataAnnotations;

namespace Trippio.Core.Models.Auth
{
    public class ResendPhoneOtpRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}