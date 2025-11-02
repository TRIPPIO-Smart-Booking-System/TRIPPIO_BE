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
        public required string PaymentMethod { get; set; } // "CreditCard", "DebitCard", "PayPal", "BankTransfer", "PayOS"

        // PayOS specific fields
        public string? PaymentLinkId { get; set; }
        public long? OrderCode { get; set; }
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
        public string? PaymentLinkId { get; set; }
        public long? OrderCode { get; set; }
        
        // Related Order information
        public OrderInfoDto? Order { get; set; }
        public BookingInfoDto? Booking { get; set; }
    }

    public class OrderInfoDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemInfoDto> OrderItems { get; set; } = new();
    }

    public class OrderItemInfoDto
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Guid? BookingId { get; set; }
        public string? BookingName { get; set; }
    }

    public class BookingInfoDto
    {
        public Guid Id { get; set; }
        public string BookingType { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class UpdatePaymentStatusRequest
    {
        public long OrderCode { get; set; }
        public string Status { get; set; } = string.Empty; // "PAID", "FAILED", "CANCELLED"
        public string? TransactionRef { get; set; }
    }
}