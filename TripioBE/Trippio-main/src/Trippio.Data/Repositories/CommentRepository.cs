using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Models;
using Trippio.Core.Repositories;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class CommentRepository : RepositoryBase<Comment, Guid>, ICommentRepository
    {
        public CommentRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Comment>> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.Comments
                .Where(c => c.BookingId == bookingId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<PageResult<Comment>> GetPagedByBookingIdAsync(Guid bookingId, int pageIndex, int pageSize)
        {
            var query = _context.Comments
                .Where(c => c.BookingId == bookingId)
                .OrderByDescending(c => c.CreatedAt);

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<Comment>
            {
                Results = items,
                CurrentPage = pageIndex,
                PageSize = pageSize,
                RowCount = totalItems
            };
        }
    }
}