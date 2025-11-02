using Trippio.Core.Domain.Entities;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface IReviewRepository : IRepository<Review, int>
    {
        Task<IEnumerable<Review>> GetReviewsByOrderIdAsync(int orderId);
        Task<IEnumerable<Review>> GetReviewsByCustomerIdAsync(Guid customerId);
        Task<Review?> GetReviewByOrderAndCustomerAsync(int orderId, Guid customerId);
        Task<bool> HasCustomerReviewedOrderAsync(int orderId, Guid customerId);
    }
}
