using Trippio.Core.Domain.Entities;
using Trippio.Core.Models;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface IFeedbackRepository : IRepository<Feedback, Guid>
    {
        Task<IEnumerable<Feedback>> GetByBookingIdAsync(Guid bookingId);
        Task<PageResult<Feedback>> GetPagedByBookingIdAsync(Guid bookingId, int pageIndex, int pageSize);
        Task<double> GetAverageRatingByBookingIdAsync(Guid bookingId);
        Task<int> GetTotalFeedbackCountByBookingIdAsync(Guid bookingId);
    }
}