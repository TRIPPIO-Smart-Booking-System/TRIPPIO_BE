using Trippio.Core.Domain.Entities;
using Trippio.Core.Models;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface IOrderRepository : IRepository<Order, int>
    {
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
        Task<Order?> GetWithItemsAsync(int id);
        Task<PageResult<Order>> GetPagedByUserIdAsync(Guid userId, int pageIndex, int pageSize);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime from, DateTime to);
        Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to);
    }

    public interface IOrderItemRepository : IRepository<OrderItem, int>
    {
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<OrderItem>> GetByProductIdAsync(int productId);
    }
}