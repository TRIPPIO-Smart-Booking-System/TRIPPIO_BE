using System.Text.Json.Serialization;


namespace Trippio.Core.Models.Basket
{
   
    public record BasketItem(
        Guid BookingId,
        int Quantity,
        decimal UnitPrice
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

    public sealed record AddItemDto(Guid BookingId, int Quantity = 1, decimal? UnitPrice = null);
    public sealed record UpdateItemQuantityDto(Guid BookingId, int Quantity);
    public sealed record RemoveItemDto(Guid BookingId);
}
