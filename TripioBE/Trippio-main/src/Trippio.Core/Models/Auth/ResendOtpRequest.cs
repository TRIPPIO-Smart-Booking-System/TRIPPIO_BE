using System.ComponentModel.DataAnnotations;

namespace Trippio.Core.Models.Auth
{
    public class ResendOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}