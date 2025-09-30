using Trippio.Core.Domain.Entities;
using Trippio.Core.Models;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface IFeedbackRepository : IRepository<Feedback, Guid>
    {
        Task<IEnumerable<Feedback>> GetByProductIdAsync(int productId);
        Task<IEnumerable<Feedback>> GetByUserIdAsync(Guid userId);
        Task<PageResult<Feedback>> GetPagedByProductIdAsync(int productId, int pageIndex, int pageSize);
        Task<double> GetAverageRatingByProductIdAsync(int productId);
        Task<IEnumerable<Feedback>> GetTopRatedAsync(int count);
        Task<IEnumerable<Feedback>> GetByDateRangeAsync(DateTime from, DateTime to);
    }
}