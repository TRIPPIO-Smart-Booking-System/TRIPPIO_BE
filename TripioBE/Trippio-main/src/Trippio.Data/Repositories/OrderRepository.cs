using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Core.Models;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class OrderRepository : RepositoryBase<Order, int>, IOrderRepository
    {
        public OrderRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<PageResult<Order>> GetPagedByUserIdAsync(Guid userId, int pageIndex, int pageSize)
        {
            var query = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User);

            var totalItems = await query.CountAsync();
            var items = await query.OrderByDescending(o => o.OrderDate)
                                  .Skip((pageIndex - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            return new PageResult<Order>
            {
                Results = items,
                CurrentPage = pageIndex,
                PageSize = pageSize,
                RowCount = totalItems
            };
        }

        public async Task<Order?> GetWithItemsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Category)
                .Include(o => o.User)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= from && o.OrderDate <= to)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= from && o.OrderDate <= to)
                .SumAsync(o => o.TotalAmount);
        }
    }
}
