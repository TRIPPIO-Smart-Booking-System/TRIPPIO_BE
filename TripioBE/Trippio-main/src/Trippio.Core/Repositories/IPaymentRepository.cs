using Trippio.Core.Domain.Entities;
using Trippio.Core.Models;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface IPaymentRepository : IRepository<Payment, Guid>
    {
        Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Payment>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<Payment>> GetByBookingIdAsync(Guid bookingId);
        Task<Payment?> GetByOrderCodeAsync(long orderCode);
        Task<PageResult<Payment>> GetPagedByUserIdAsync(Guid userId, int pageIndex, int pageSize);
        Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime from, DateTime to);
        Task<decimal> GetTotalPaymentAmountAsync(DateTime from, DateTime to);
    }
}