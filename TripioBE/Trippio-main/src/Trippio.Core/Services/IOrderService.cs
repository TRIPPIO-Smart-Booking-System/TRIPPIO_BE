using Trippio.Core.Models;
using Trippio.Core.Models.Basket;
using Trippio.Core.Models.Common;
using Trippio.Core.Models.Order;

namespace Trippio.Core.Services
{
    public interface IOrderService
    {
        Task<BaseResponse<IEnumerable<OrderDto>>> GetAllAsync();
        Task<BaseResponse<IEnumerable<OrderDto>>> GetByUserIdAsync(Guid userId);
        Task<BaseResponse<OrderDto>> GetByIdAsync(int id);
        Task<BaseResponse<IEnumerable<OrderDto>>> GetByStatusAsync(string status);
        Task<BaseResponse<OrderDto>> UpdateStatusAsync(int id, string status);
        Task<BaseResponse<bool>> CancelOrderAsync(int id, Guid userId);
        Task<BaseResponse<decimal>> GetTotalRevenueAsync(DateTime from, DateTime to);
        Task<BaseResponse<IEnumerable<OrderDto>>> GetPendingOrdersAsync();
        Task<BaseResponse<OrderDto>> CreateFromBasketAsync(Guid userId, CancellationToken ct = default);
        Task<BaseResponse<OrderDto>> CreateOrderAsync(CreateOrderRequest request);
    }
}