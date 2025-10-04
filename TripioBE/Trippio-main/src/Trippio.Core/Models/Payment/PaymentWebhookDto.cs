namespace Trippio.Core.Models.Payment
{
    public class PaymentWebhookDto
    {
        public Guid PaymentId { get; set; }
        public int? OrderId { get; set; }
        public Guid? BookingId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; 
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    }
}
