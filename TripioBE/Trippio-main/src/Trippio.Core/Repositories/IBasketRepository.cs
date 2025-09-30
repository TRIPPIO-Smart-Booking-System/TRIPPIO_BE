using Trippio.Core.Domain.Entities;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface IBasketRepository : IRepository<Basket, Guid>
    {
        Task<Basket?> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Basket>> GetActiveCartsAsync();
        Task<IEnumerable<Basket>> GetExpiredCartsAsync(DateTime expiredBefore);
        Task DeleteExpiredCartsAsync(DateTime expiredBefore);
        Task<int> GetCartItemCountAsync(Guid userId);
        Task<decimal> GetCartTotalAsync(Guid userId);
    }
}