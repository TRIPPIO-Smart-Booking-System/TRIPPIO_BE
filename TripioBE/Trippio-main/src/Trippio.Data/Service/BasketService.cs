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
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public BasketService(IConnectionMultiplexer mux) => _redis = mux.GetDatabase();

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
            if (dto.BookingId == Guid.Empty) return BaseResponse<Basket>.Error("bookingId is required", 400);
            if (dto.Quantity <= 0) return BaseResponse<Basket>.Error("quantity must be > 0", 400);

            var cur = (await GetAsync(userId, ct)).Data!;
            var items = cur.Items.ToList();
            var exist = items.FirstOrDefault(i => i.BookingId == dto.BookingId);

            if (exist is null)
                items.Add(new BasketItem(dto.BookingId, dto.Quantity, dto.UnitPrice ?? 0m));
            else
            {
                var idx = items.IndexOf(exist);
                items[idx] = exist with { Quantity = exist.Quantity + dto.Quantity };
            }

            var updated = new Basket(userId, items);
            await _redis.StringSetAsync(Key(userId), JsonSerializer.Serialize(updated, _json), TimeSpan.FromDays(7));
            return BaseResponse<Basket>.Success(updated, "Basket updated");
        }

        public async Task<BaseResponse<Basket>> UpdateQuantityAsync(Guid userId, UpdateItemQuantityDto dto, CancellationToken ct = default)
        {
            if (dto.Quantity < 0) return BaseResponse<Basket>.Error("quantity must be >= 0", 400);

            var cur = (await GetAsync(userId, ct)).Data!;
            var items = cur.Items.ToList();
            var exist = items.FirstOrDefault(i => i.BookingId == dto.BookingId);
            if (exist is null) return BaseResponse<Basket>.NotFound("Item not found in basket");

            if (dto.Quantity == 0) items.Remove(exist);
            else
            {
                var idx = items.IndexOf(exist);
                items[idx] = exist with { Quantity = dto.Quantity };
            }

            var updated = new Basket(userId, items);
            await _redis.StringSetAsync(Key(userId), JsonSerializer.Serialize(updated, _json), TimeSpan.FromDays(7));
            return BaseResponse<Basket>.Success(updated, "Basket updated");
        }

        public async Task<BaseResponse<Basket>> RemoveItemAsync(Guid userId, Guid bookingId, CancellationToken ct = default)
        {
            var cur = (await GetAsync(userId, ct)).Data!;
            var items = cur.Items.Where(i => i.BookingId != bookingId).ToList();
            if (items.Count == cur.Items.Count) return BaseResponse<Basket>.NotFound("Item not found in basket");

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
