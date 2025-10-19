namespace Trippio.Core.Models.Payment
{
    /// <summary>
    /// Response from PayOS payment creation
    /// </summary>
    public class PayOSPaymentResponse
    {
        /// <summary>
        /// Payment checkout URL for user to complete payment
        /// </summary>
        public string CheckoutUrl { get; set; } = string.Empty;

        /// <summary>
        /// Order code for tracking
        /// </summary>
        public long OrderCode { get; set; }

        /// <summary>
        /// Payment amount
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// QR code data (base64 or URL)
        /// </summary>
        public string? QrCode { get; set; }

        /// <summary>
        /// Payment ID from PayOS
        /// </summary>
        public string? PaymentLinkId { get; set; }

        /// <summary>
        /// Status of the payment link creation
        /// </summary>
        public string Status { get; set; } = "PENDING";
    }
}
