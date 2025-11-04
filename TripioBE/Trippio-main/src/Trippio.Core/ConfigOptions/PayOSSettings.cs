namespace Trippio.Core.ConfigOptions
{
    /// <summary>
    /// PayOS configuration settings for real money payment integration
    /// Get these values from PayOS dashboard: https://my.payos.vn
    /// </summary>
    public class PayOSSettings
    {
        /// <summary>
        /// Client ID from PayOS (e.g., "a6e41e2d-81b1-456c-abcd-123456789012")
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// API Key from PayOS (e.g., "abc123-def456-ghi789")
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Checksum Key for signature validation (e.g., "xyz789-uvw456-rst123")
        /// </summary>
        public string ChecksumKey { get; set; } = string.Empty;

        /// <summary>
        /// Return URL after successful payment for Web (e.g., "http://localhost:3000/payment-success")
        /// </summary>
        public string WebReturnUrl { get; set; } = string.Empty;

        /// <summary>
        /// Cancel URL if user cancels payment for Web (e.g., "http://localhost:3000/payment-cancel")
        /// </summary>
        public string WebCancelUrl { get; set; } = string.Empty;

        /// <summary>
        /// Return URL after successful payment for Mobile (e.g., "trippio://payment/success")
        /// </summary>
        public string MobileReturnUrl { get; set; } = string.Empty;

        /// <summary>
        /// Cancel URL if user cancels payment for Mobile (e.g., "trippio://payment/cancel")
        /// </summary>
        public string MobileCancelUrl { get; set; } = string.Empty;

        /// <summary>
        /// Webhook URL for PayOS callback (e.g., "https://your-backend.com/api/payment/payos-callback")
        /// </summary>
        public string WebhookUrl { get; set; } = string.Empty;

        // Legacy properties for backward compatibility
        [Obsolete("Use WebReturnUrl instead")]
        public string ReturnUrl
        {
            get => WebReturnUrl;
            set => WebReturnUrl = value;
        }

        [Obsolete("Use WebCancelUrl instead")]
        public string CancelUrl
        {
            get => WebCancelUrl;
            set => WebCancelUrl = value;
        }
    }
}
