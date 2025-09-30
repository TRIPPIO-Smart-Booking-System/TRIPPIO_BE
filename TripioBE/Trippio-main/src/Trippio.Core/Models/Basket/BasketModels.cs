using System.ComponentModel.DataAnnotations;

namespace Trippio.Core.Models.Basket
{
    public class BasketItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public class BasketDto
    {
        public Guid UserId { get; set; }
        public List<BasketItem> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(x => x.Price * x.Quantity);
        public int TotalItems => Items.Sum(x => x.Quantity);
        public DateTime LastUpdated { get; set; }
    }

    public class AddToBasketRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }
    }

    public class UpdateBasketItemRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }
    }

    public class RemoveFromBasketRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int ProductId { get; set; }
    }
}