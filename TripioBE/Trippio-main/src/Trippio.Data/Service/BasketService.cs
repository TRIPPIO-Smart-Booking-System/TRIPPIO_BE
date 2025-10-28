using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Trippio.Core.Models.Basket;
using Trippio.Core.Models.Common;
using Trippio.Core.Services;

namespace Trippio.Data.Service
{
    public class BasketService : IBasketService
    {
        private readonly IDatabase _redis;
        private readonly TrippioDbContext _db;
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public BasketService(IConnectionMultiplexer mux, TrippioDbContext db)
        {
            _redis = mux.GetDatabase();
            _db = db;
        }

        private static string Key(Guid userId) => $"basket:{userId}";

        public async Task<BaseResponse<Basket>> GetAsync(Guid userId, CancellationToken ct = default)
        {
            var raw = await _redis.StringGetAsync(Key(userId));
            if (raw.IsNullOrEmpty) return BaseResponse<Basket>.Success(new Basket(userId));
            var basket = JsonSerializer.Deserialize<Basket>(raw!, _json) ?? new Basket(userId);
            return BaseResponse<Basket>.Success(basket);
        }

        public async Task<BaseResponse<Basket>> AddItemAsync(Guid userId, AddItemDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.ProductId))
                return BaseResponse<Basket>.Error("productId is required", 400);
            if (dto.Quantity <= 0)
                return BaseResponse<Basket>.Error("quantity must be > 0", 400);

            
            var price = await GetProductPriceAsync(dto.ProductId, ct);
            if (price == null)
                return BaseResponse<Basket>.Error("Invalid productId or product not found", 404);

            var cur = (await GetAsync(userId, ct)).Data!;
            var items = cur.Items.ToList();

            JsonDocument? attrsDoc = null;
            if (dto.Attributes.HasValue && dto.Attributes.Value.ValueKind != JsonValueKind.Undefined && dto.Attributes.Value.ValueKind != JsonValueKind.Null)
                attrsDoc = JsonDocument.Parse(dto.Attributes.Value.GetRawText());

            var newItem = new BasketItem(dto.ProductId, dto.Quantity, price.Value, attrsDoc);

            
            string AttrKey(JsonDocument? d) => d is null ? "" : d.RootElement.GetRawText();
            string keyOf(BasketItem i) => $"{i.ProductId}|{AttrKey(i.Attributes)}";

            var exist = items.FirstOrDefault(i => keyOf(i) == keyOf(newItem));
            if (exist is null)
                items.Add(newItem);
            else
            {
                var idx = items.IndexOf(exist);
                items[idx] = exist with
                {
                    Quantity = exist.Quantity + dto.Quantity,
                    Price = price.Value, 
                    
                };
            }

            var updated = new Basket(userId, items);
            await _redis.StringSetAsync(Key(userId), JsonSerializer.Serialize(updated, _json), TimeSpan.FromDays(7));
            return BaseResponse<Basket>.Success(updated, "Basket updated");
        }

        private async Task<decimal?> GetProductPriceAsync(string productId, CancellationToken ct)
        {
            if (!Guid.TryParse(productId, out var id))
                return null;

           
            var room = await _db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);
            if (room != null) return room.PricePerNight;

            
            var trip = await _db.TransportTrips.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);
            if (trip != null) return trip.Price;

            
            var show = await _db.Shows.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
            if (show != null) return show.Price;

            
            var service = await _db.ExtraServices.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
            if (service != null) return service.Price;

            return null;
        }

        public async Task<BaseResponse<Basket>> UpdateQuantityAsync(Guid userId, UpdateItemQuantityDto dto, CancellationToken ct = default)
        {
            if (dto.Quantity < 0)
                return BaseResponse<Basket>.Error("quantity must be >= 0", 400);

            var cur = (await GetAsync(userId, ct)).Data!;
            var items = cur.Items.ToList();
            var exist = items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (exist is null)
                return BaseResponse<Basket>.NotFound("Item not found");

            if (dto.Quantity == 0)
                items.Remove(exist);
            else
            {
                var idx = items.IndexOf(exist);
                items[idx] = exist with { Quantity = dto.Quantity };
            }

            var updated = new Basket(userId, items);
            await _redis.StringSetAsync(Key(userId), JsonSerializer.Serialize(updated, _json), TimeSpan.FromDays(7));
            return BaseResponse<Basket>.Success(updated, "Basket updated");
        }

        public async Task<BaseResponse<Basket>> RemoveItemAsync(Guid userId, string productId, CancellationToken ct = default)
        {
            var cur = (await GetAsync(userId, ct)).Data!;
            var exist = cur.Items.FirstOrDefault(i => i.ProductId == productId);
            if (exist is null)
                return BaseResponse<Basket>.NotFound("Item not found");

            var items = cur.Items.Where(i => i.ProductId != productId).ToList();
            var updated = new Basket(userId, items);
            await _redis.StringSetAsync(Key(userId), JsonSerializer.Serialize(updated, _json), TimeSpan.FromDays(7));
            return BaseResponse<Basket>.Success(updated, "Item removed");
        }

        public async Task<BaseResponse<bool>> ClearAsync(Guid userId, CancellationToken ct = default)
        {
            await _redis.KeyDeleteAsync(Key(userId));
            return BaseResponse<bool>.Success(true, "Basket cleared");
        }
    }
}
