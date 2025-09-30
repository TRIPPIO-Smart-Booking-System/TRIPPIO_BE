using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Core.Models;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class BasketRepository : RepositoryBase<Basket, Guid>, IBasketRepository
    {
        public BasketRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<Basket?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Baskets
                .FirstOrDefaultAsync(b => b.UserId == userId);
        }

        public async Task<IEnumerable<Basket>> GetActiveCartsAsync()
        {
            return await _context.Baskets
                .Where(b => b.UpdatedAt > DateTime.UtcNow.AddDays(-7)) // Active within last 7 days
                .ToListAsync();
        }

        public async Task<IEnumerable<Basket>> GetExpiredCartsAsync(DateTime expiredBefore)
        {
            return await _context.Baskets
                .Where(b => b.UpdatedAt < expiredBefore)
                .ToListAsync();
        }

        public async Task DeleteExpiredCartsAsync(DateTime expiredBefore)
        {
            var expiredCarts = await GetExpiredCartsAsync(expiredBefore);
            _context.Baskets.RemoveRange(expiredCarts);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCartItemCountAsync(Guid userId)
        {
            var basket = await GetByUserIdAsync(userId);
            if (basket?.Items == null) return 0;
            
            // Parse JSON to count items (simplified - in real implementation you'd use proper JSON parsing)
            return basket.Items.Split(',').Length; // This is a simplified approach
        }

        public async Task<decimal> GetCartTotalAsync(Guid userId)
        {
            var basket = await GetByUserIdAsync(userId);
            return basket?.TotalAmount ?? 0;
        }
    }
}
