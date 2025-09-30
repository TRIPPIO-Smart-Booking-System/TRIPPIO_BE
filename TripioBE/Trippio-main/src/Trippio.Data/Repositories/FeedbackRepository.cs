using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Core.Models;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class FeedbackRepository : RepositoryBase<Feedback, int>, IFeedbackRepository
    {
        public FeedbackRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Feedback>> GetByProductIdAsync(int productId)
        {
            return await _context.Feedbacks
                .Where(f => f.ProductId == productId)
                .Include(f => f.Product)
                .OrderByDescending(f => f.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Feedbacks
                .Include(f => f.Product)
                .OrderByDescending(f => f.DateCreated)
                .ToListAsync();
        }

        public async Task<PageResult<Feedback>> GetPagedByProductIdAsync(int productId, int pageIndex, int pageSize)
        {
            var query = _context.Feedbacks
                .Where(f => f.ProductId == productId)
                .Include(f => f.Product);

            var totalItems = await query.CountAsync();
            var items = await query.OrderByDescending(f => f.DateCreated)
                                  .Skip((pageIndex - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            return new PageResult<Feedback>
            {
                Results = items,
                RowCount = totalItems,
                CurrentPage = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<double> GetAverageRatingByProductIdAsync(int productId)
        {
            var ratings = await _context.Feedbacks
                .Where(f => f.ProductId == productId)
                .Select(f => f.Rating)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0;
        }

        public async Task<IEnumerable<Feedback>> GetTopRatedAsync(int count)
        {
            return await _context.Feedbacks
                .Include(f => f.Product)
                .OrderByDescending(f => f.Rating)
                .ThenByDescending(f => f.DateCreated)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _context.Feedbacks
                .Where(f => f.DateCreated >= from && f.DateCreated <= to)
                .Include(f => f.Product)
                .OrderByDescending(f => f.DateCreated)
                .ToListAsync();
        }
    }
}
