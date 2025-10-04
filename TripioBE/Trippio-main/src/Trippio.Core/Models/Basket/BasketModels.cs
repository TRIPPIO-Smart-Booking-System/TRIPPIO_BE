using System.Text.Json.Serialization;

namespace Trippio.Core.Models.Basket
{
    public record BasketItem(
        string ProductId,
        int Quantity,
        decimal Price
    );

    public class Basket
    {
        public Guid UserId { get; init; }
        public IList<BasketItem> Items { get; init; } = new List<BasketItem>();

        [JsonIgnore]
        public decimal Total => Items.Sum(i => i.Price * i.Quantity);

        public Basket(Guid userId) => UserId = userId;
        [JsonConstructor] public Basket(Guid userId, IList<BasketItem> items) { UserId = userId; Items = items ?? new List<BasketItem>(); }
    }

    public record AddItemDto(string ProductId, int Quantity, decimal Price);
    public record UpdateItemQuantityDto(string ProductId, int Quantity);
    public record RemoveItemDto(string ProductId);
}
