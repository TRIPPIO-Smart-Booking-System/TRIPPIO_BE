using Trippio.Core.Domain.Entities;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface IReviewRepository : IRepository<Review, int>
    {
        Task<IEnumerable<Review>> GetAllReviewsAsync();
        Task<IEnumerable<Review>> GetReviewsByOrderIdAsync(int orderId);
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(Guid userId);
        Task<Review?> GetReviewByOrderAndUserAsync(int orderId, Guid userId);
        Task<bool> HasUserReviewedOrderAsync(int orderId, Guid userId);
    }
}
