using System.ComponentModel.DataAnnotations;

namespace Trippio.Core.Models.Payment
{
    public class CreatePaymentRequest
    {
        [Required]
        public Guid UserId { get; set; }

        public int? OrderId { get; set; }

        public Guid? BookingId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(100)]
        public required string PaymentMethod { get; set; } // "CreditCard", "DebitCard", "PayPal", "BankTransfer"
    }

    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int? OrderId { get; set; }
        public Guid? BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaidAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}