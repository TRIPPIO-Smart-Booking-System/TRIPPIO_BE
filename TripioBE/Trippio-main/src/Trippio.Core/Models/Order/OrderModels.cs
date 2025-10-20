using System.ComponentModel.DataAnnotations;

namespace Trippio.Core.Models.Order
{
    public class CreateOrderRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public List<CreateOrderItemRequest> OrderItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    public class CreateOrderItemRequest
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new();
        public string Status { get; set; } = string.Empty;
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Guid BookingId { get; set; }
        public string BookingName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }


}