using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Models.Basket;
using Trippio.Core.Models.Common;
using Trippio.Core.Models.Order;
using Trippio.Core.Repositories;
using Trippio.Core.SeedWorks;
using Trippio.Core.Services;
using Trippio.Data.Repositories;
using Trippio.Data.SeedWorks;
using OrderEntity = Trippio.Core.Domain.Entities.Order;
using OrderItemEntity = Trippio.Core.Domain.Entities.OrderItem;

namespace Trippio.Data.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly TrippioDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IBasketService _basket;

        public OrderService(IOrderRepository orderRepo, TrippioDbContext db, IBookingRepository bookingRepo, IUnitOfWork uow, IMapper mapper, IBasketService basket)
        {
            _orderRepo = orderRepo;
            _bookingRepo = bookingRepo;
            _db = db;
            _uow = uow;
            _mapper = mapper;
            _basket = basket;
        }

        public async Task<BaseResponse<IEnumerable<OrderDto>>> GetByUserIdAsync(Guid userId)
        {
            var data = await _orderRepo.Query()
        .Where(o => o.UserId == userId)
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Booking)
        .OrderByDescending(o => o.OrderDate)
        .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
        .ToListAsync();

            return BaseResponse<IEnumerable<OrderDto>>.Success(data);
        }

        public async Task<BaseResponse<OrderDto>> GetByIdAsync(int id)
        {
            var entity = await _orderRepo.Query()
        .Include(o => o.OrderItems).ThenInclude(oi => oi.Booking)
        .Include(o => o.Payments)
        .FirstOrDefaultAsync(o => o.Id == id);

            if (entity is null)
                return BaseResponse<OrderDto>.NotFound($"Order #{id} not found");

            return BaseResponse<OrderDto>.Success(_mapper.Map<OrderDto>(entity));
        }

        public async Task<BaseResponse<IEnumerable<OrderDto>>> GetByStatusAsync(string status)
        {
            if (!TryParseStatus(status, out var parsed))
                return BaseResponse<IEnumerable<OrderDto>>.Error($"Unknown status '{status}'", code: 400);

            var data = await _orderRepo.Query()
                .Where(o => o.Status == parsed)
                .OrderBy(o => o.OrderDate)
                .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return BaseResponse<IEnumerable<OrderDto>>.Success(data);
        }

        public async Task<BaseResponse<OrderDto>> UpdateStatusAsync(int id, string status)
        {
            if (!TryParseStatus(status, out var parsed))
                return BaseResponse<OrderDto>.Error($"Unknown status '{status}'", code: 400);

            var entity = await _orderRepo.FindByIdAsync(id);
            if (entity is null)
                return BaseResponse<OrderDto>.NotFound($"Order #{id} not found");

            entity.Status = parsed;
            entity.ModifiedDate = DateTime.UtcNow;

            _orderRepo.Update(entity);
            await _uow.CompleteAsync();

            return BaseResponse<OrderDto>.Success(_mapper.Map<OrderDto>(entity), "Order status updated");
        }

        public async Task<BaseResponse<bool>> CancelOrderAsync(int id, Guid userId)
        {
            var entity = await _orderRepo.FindByIdAsync(id);
            if (entity is null)
                return BaseResponse<bool>.NotFound($"Order #{id} not found");

            if (entity.UserId != userId)
                return BaseResponse<bool>.Error("You cannot cancel someone else's order", code: 403);

            if (entity.Status == OrderStatus.Cancelled)
                return BaseResponse<bool>.Success(true, "Order already cancelled");

            if (entity.Status != OrderStatus.Pending)
                return BaseResponse<bool>.Error("Only pending orders can be cancelled", code: 409);

            entity.Status = OrderStatus.Cancelled;
            entity.ModifiedDate = DateTime.UtcNow;

            _orderRepo.Update(entity);
            await _uow.CompleteAsync();

            return BaseResponse<bool>.Success(true, "Order cancelled");
        }

        public async Task<BaseResponse<decimal>> GetTotalRevenueAsync(DateTime from, DateTime to)
        {
            if (to < from)
                return BaseResponse<decimal>.Error("End date must be after start date", code: 400);

            var total = await _orderRepo.Query()
                .Where(o => o.Status == OrderStatus.Confirmed &&
                            o.OrderDate >= from && o.OrderDate < to)
                .SumAsync(o => o.TotalAmount);

            return BaseResponse<decimal>.Success(total, "Revenue calculated");
        }

        public async Task<BaseResponse<IEnumerable<OrderDto>>> GetPendingOrdersAsync()
        {
            var data = await _orderRepo.Query()
                .Where(o => o.Status == OrderStatus.Pending)
                .OrderBy(o => o.OrderDate)
                .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return BaseResponse<IEnumerable<OrderDto>>.Success(data);
        }

        private static bool TryParseStatus(string status, out OrderStatus parsed)
            => Enum.TryParse(status?.Trim(), ignoreCase: true, out parsed);

        public async Task<BaseResponse<OrderDto>> CreateFromBasketAsync(Guid userId, CancellationToken ct = default)
        {
            // 1) Lấy basket
            var basketResp = await _basket.GetAsync(userId, ct);
            var basket = basketResp.Data;
            if (basket == null || basket.Items.Count == 0)
                return BaseResponse<OrderDto>.Error("Basket is empty", 400);

            // 2) Giao dịch để đảm bảo atomic (tạo bookings + order)
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // 2.1) Tạo BOOKINGS từ basket items (vì OrderItem đang FK tới BookingId)
                var now = DateTime.UtcNow;
                var newBookings = new List<Booking>();
                foreach (var i in basket.Items)
                {
                    var b = new Booking
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        // Vì giỏ “không phân biệt loại”, ta gán 1 loại chung.
                        // Nếu bạn muốn tên khác ("BasketItem" / "Composite") thì đổi chuỗi này.
                        BookingType = "Misc",
                        Status = "Pending",
                        BookingDate = now,
                        // tổng tiền cho booking này = unit * qty của item
                        TotalAmount = i.UnitPrice * i.Quantity,
                        DateCreated = now
                    };
                    newBookings.Add(b);
                }
                _db.Bookings.AddRange(newBookings);

                // 2.2) Tạo ORDER + ORDER ITEMS trỏ tới các Booking vừa thêm
                var orderItems = newBookings
                    .Zip(basket.Items, (b, i) => new OrderItemEntity
                    {
                        BookingId = b.Id,
                        Quantity = i.Quantity,
                        Price = i.UnitPrice
                    })
                    .ToList();

                var total = orderItems.Sum(x => x.Price * x.Quantity);

                var order = new OrderEntity
                {
                    UserId = userId,
                    OrderDate = now,
                    Status = OrderStatus.Pending,
                    DateCreated = now,
                    TotalAmount = total,
                    OrderItems = orderItems
                };

                await _orderRepo.AddAsync(order);
                await _uow.CompleteAsync(); // lưu bookings + order + items

                await tx.CommitAsync(ct);

                // 2.3) Clear basket
                await _basket.ClearAsync(userId, ct);

                // 2.4) Load lại order đầy đủ để trả về
                var loaded = await _orderRepo.Query()
                    .Where(o => o.Id == order.Id)
                    .Include(o => o.OrderItems).ThenInclude(oi => oi.Booking)
                    .SingleAsync(ct);

                return BaseResponse<OrderDto>.Success(_mapper.Map<OrderDto>(loaded),
                    "Order created from basket");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                return BaseResponse<OrderDto>.Error($"Failed to create order from basket: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<OrderDto>> CreateOrderAsync(CreateOrderRequest request)
{
    if (request.OrderItems == null || request.OrderItems.Count == 0)
        return BaseResponse<OrderDto>.Error("OrderItems is empty.", 400);
            var bookingIds = request.OrderItems.Select(i => i.BookingId).Distinct().ToList();

            var existed = await _db.Bookings
                .Where(b => bookingIds.Contains(b.Id))
                .Select(b => b.Id)
                .ToListAsync();

            var missing = bookingIds.Except(existed).ToList();
            if (missing.Any())
                return BaseResponse<OrderDto>.Error(
                    $"Invalid BookingId(s): {string.Join(", ", missing)}", 400);
            var total = request.OrderItems.Sum(i => i.Price * i.Quantity);

    var order = new OrderEntity
    {
        UserId      = request.UserId,
        OrderDate   = DateTime.UtcNow,
        Status      = OrderStatus.Pending,
        DateCreated = DateTime.UtcNow,
        TotalAmount = total,
        OrderItems  = request.OrderItems.Select(oi => new OrderItemEntity
        {
            BookingId = oi.BookingId,
            Quantity  = oi.Quantity,
            Price     = oi.Price
        }).ToList()
    };

    await _orderRepo.AddAsync(order);
    await _uow.CompleteAsync();

    var loaded = await _orderRepo.Query()
        .Where(o => o.Id == order.Id)
        .Include(o => o.OrderItems).ThenInclude(oi => oi.Booking)
        .SingleAsync();

    return BaseResponse<OrderDto>.Success(
        _mapper.Map<OrderDto>(loaded), "Order created");
}


        }


}
