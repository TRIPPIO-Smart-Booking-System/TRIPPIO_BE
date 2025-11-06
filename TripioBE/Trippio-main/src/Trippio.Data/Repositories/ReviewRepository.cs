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
                .Include(r => r.Customer)
                .Include(r => r.Order)
                .AsSplitQuery()
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByOrderIdAsync(int orderId)
        {
            return await _context.Reviews
                .Include(r => r.Customer)
                .Where(r => r.OrderId == orderId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByCustomerIdAsync(Guid customerId)
        {
            return await _context.Reviews
                .Include(r => r.Order)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetReviewByOrderAndCustomerAsync(int orderId, Guid customerId)
        {
            return await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.OrderId == orderId && r.CustomerId == customerId);
        }

        public async Task<bool> HasCustomerReviewedOrderAsync(int orderId, Guid customerId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.OrderId == orderId && r.CustomerId == customerId);
        }
    }
}
