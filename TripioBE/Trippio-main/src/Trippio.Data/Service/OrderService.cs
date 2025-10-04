using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Models.Basket;
using Trippio.Core.Models.Common;
using Trippio.Core.Models.Order;
using Trippio.Core.SeedWorks;
using Trippio.Core.Services;
using Trippio.Data.Repositories;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepo, IUnitOfWork uow, IMapper mapper)
        {
            _orderRepo = orderRepo;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<BaseResponse<IEnumerable<OrderDto>>> GetByUserIdAsync(Guid userId)
        {
            var data = await _orderRepo.Query()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return BaseResponse<IEnumerable<OrderDto>>.Success(data);
        }

        public async Task<BaseResponse<OrderDto>> GetByIdAsync(int id)
        {
            var entity = await _orderRepo.Query()
                .Include(o => o.OrderItems)
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

        public async Task<BaseResponse<OrderDto>> CreateFromBasketAsync(Guid userId, Basket basket, CancellationToken ct = default)
        {
            if (basket == null || basket.Items.Count == 0)
                return BaseResponse<OrderDto>.Error("Basket is empty", 400);

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = basket.Total,
                Status = OrderStatus.Pending,
                DateCreated = DateTime.UtcNow
            };

            await _orderRepo.Add(order);         
            await _uow.CompleteAsync();         

            var dto = _mapper.Map<OrderDto>(order);
            return BaseResponse<OrderDto>.Success(dto, "Order created from basket");
        }
    }


}
