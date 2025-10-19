using System.ComponentModel.DataAnnotations;

namespace Trippio.Core.Models.Payment
{
    /// <summary>
    /// Request model for creating PayOS real money payment
    /// </summary>
    public class CreatePayOSPaymentRequest
    {
        /// <summary>
        /// Order code - unique identifier for the transaction (max 6 digits for PayOS)
        /// Example: 123456
        /// </summary>
        [Required]
        [Range(1, 999999)]
        public long OrderCode { get; set; }

        /// <summary>
        /// Payment amount in VND (minimum 2000 VND)
        /// </summary>
        [Required]
        [Range(2000, int.MaxValue, ErrorMessage = "Amount must be at least 2000 VND")]
        public int Amount { get; set; }

        /// <summary>
        /// Description of the payment
        /// Example: "Payment for booking #123"
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Buyer's name (optional)
        /// </summary>
        [MaxLength(100)]
        public string? BuyerName { get; set; }

        /// <summary>
        /// Buyer's email (optional)
        /// </summary>
        [EmailAddress]
        public string? BuyerEmail { get; set; }

        /// <summary>
        /// Buyer's phone (optional)
        /// </summary>
        [Phone]
        public string? BuyerPhone { get; set; }

        /// <summary>
        /// User ID making the payment (for tracking)
        /// </summary>
        public Guid? UserId { get; set; }
    }
}
