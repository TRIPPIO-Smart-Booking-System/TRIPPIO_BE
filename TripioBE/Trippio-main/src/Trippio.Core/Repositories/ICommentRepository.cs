using Trippio.Core.Domain.Entities;
using Trippio.Core.Models;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface ICommentRepository : IRepository<Comment, Guid>
    {
        Task<IEnumerable<Comment>> GetByBookingIdAsync(Guid bookingId);
        Task<PageResult<Comment>> GetPagedByBookingIdAsync(Guid bookingId, int pageIndex, int pageSize);
    }
}