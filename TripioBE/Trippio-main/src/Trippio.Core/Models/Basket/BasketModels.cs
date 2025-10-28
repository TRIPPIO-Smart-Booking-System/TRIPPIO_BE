using System.Text.Json;
using System.Text.Json.Serialization;


namespace Trippio.Core.Models.Basket
{
   
    public record BasketItem(
        //Guid BookingId,
        string ProductId,
        int Quantity,
        decimal UnitPrice,
        JsonDocument? Attributes = null
    );

    public class Basket
    {
        public Guid UserId { get; init; }
        public IList<BasketItem> Items { get; init; } = new List<BasketItem>();

        [JsonIgnore]
        public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);

        public Basket(Guid userId) => UserId = userId;

        [JsonConstructor]
        public Basket(Guid userId, IList<BasketItem> items)
        {
            UserId = userId;
            Items = items ?? new List<BasketItem>();
        }
    }

    public record AddItemDto(string ProductId, int Quantity, decimal UnitPrice, JsonElement? Attributes = null );
    public sealed record UpdateItemQuantityDto(string ProductId, int Quantity);
    public sealed record RemoveItemDto(string ProductId);
}
