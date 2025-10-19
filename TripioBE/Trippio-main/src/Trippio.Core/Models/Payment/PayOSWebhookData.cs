using System.Text.Json.Serialization;

namespace Trippio.Core.Models.Payment
{
    /// <summary>
    /// Webhook payload from PayOS when payment status changes
    /// Reference: https://payos.vn/docs/api/webhook
    /// </summary>
    public class PayOSWebhookData
    {
        [JsonPropertyName("orderCode")]
        public long OrderCode { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("accountNumber")]
        public string? AccountNumber { get; set; }

        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("transactionDateTime")]
        public string TransactionDateTime { get; set; } = string.Empty;

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "VND";

        [JsonPropertyName("paymentLinkId")]
        public string PaymentLinkId { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;  // "00" = success

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;

        [JsonPropertyName("counterAccountBankId")]
        public string? CounterAccountBankId { get; set; }

        [JsonPropertyName("counterAccountBankName")]
        public string? CounterAccountBankName { get; set; }

        [JsonPropertyName("counterAccountName")]
        public string? CounterAccountName { get; set; }

        [JsonPropertyName("counterAccountNumber")]
        public string? CounterAccountNumber { get; set; }

        [JsonPropertyName("virtualAccountName")]
        public string? VirtualAccountName { get; set; }

        [JsonPropertyName("virtualAccountNumber")]
        public string? VirtualAccountNumber { get; set; }
    }

    /// <summary>
    /// Full webhook request from PayOS including signature
    /// </summary>
    public class PayOSWebhookRequest
    {
        [JsonPropertyName("data")]
        public PayOSWebhookData Data { get; set; } = new();

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;
    }
}
