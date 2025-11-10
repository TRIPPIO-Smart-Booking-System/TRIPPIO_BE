using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class ReviewRepository : RepositoryBase<Review, int>, IReviewRepository
    {
        public ReviewRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Review>> GetAllReviewsAsync()
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Order)
                .AsSplitQuery()
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByOrderIdAsync(int orderId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.OrderId == orderId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(Guid userId)
        {
            return await _context.Reviews
                .Include(r => r.Order)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetReviewByOrderAndUserAsync(int orderId, Guid userId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.OrderId == orderId && r.UserId == userId);
        }

        public async Task<bool> HasUserReviewedOrderAsync(int orderId, Guid userId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.OrderId == orderId && r.UserId == userId);
        }
    }
}
