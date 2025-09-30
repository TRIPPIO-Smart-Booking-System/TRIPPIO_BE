using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Core.Models;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class PaymentRepository : RepositoryBase<Payment, Guid>, IPaymentRepository
    {
        public PaymentRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .Include(p => p.Order)
                .Include(p => p.Booking)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.Payments
                .Where(p => p.BookingId == bookingId)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task<PageResult<Payment>> GetPagedByUserIdAsync(Guid userId, int pageIndex, int pageSize)
        {
            var query = _context.Payments
                .Where(p => p.UserId == userId)
                .Include(p => p.Order)
                .Include(p => p.Booking);

            var totalItems = await query.CountAsync();
            var items = await query.OrderByDescending(p => p.PaidAt)
                                  .Skip((pageIndex - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            return new PageResult<Payment>
            {
                Results = items,
                RowCount = totalItems,
                CurrentPage = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _context.Payments
                .Where(p => p.PaidAt >= from && p.PaidAt <= to)
                .Include(p => p.Order)
                .Include(p => p.Booking)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaymentAmountAsync(DateTime from, DateTime to)
        {
            return await _context.Payments
                .Where(p => p.PaidAt >= from && p.PaidAt <= to)
                .SumAsync(p => p.Amount);
        }
    }
}
