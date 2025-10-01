using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Models;
using Trippio.Core.Repositories;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class FeedbackRepository : RepositoryBase<Feedback, Guid>, IFeedbackRepository
    {
        public FeedbackRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Feedback>> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.Feedbacks
                .Where(f => f.BookingId == bookingId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<PageResult<Feedback>> GetPagedByBookingIdAsync(Guid bookingId, int pageIndex, int pageSize)
        {
            var query = _context.Feedbacks
                .Where(f => f.BookingId == bookingId)
                .OrderByDescending(f => f.CreatedAt);

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<Feedback>
            {
                Results = items,
                CurrentPage = pageIndex,
                PageSize = pageSize,
                RowCount = totalItems
            };
        }

        public async Task<double> GetAverageRatingByBookingIdAsync(Guid bookingId)
        {
            var feedbacks = await _context.Feedbacks
                .Where(f => f.BookingId == bookingId)
                .ToListAsync();

            return feedbacks.Any() ? feedbacks.Average(f => f.Rating) : 0;
        }

        public async Task<int> GetTotalFeedbackCountByBookingIdAsync(Guid bookingId)
        {
            return await _context.Feedbacks
                .CountAsync(f => f.BookingId == bookingId);
        }
    }
}