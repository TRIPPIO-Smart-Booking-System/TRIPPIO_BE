using Trippio.Core.Domain.Entities;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface IExtraServiceRepository : IRepository<ExtraService, Guid>
    {
        Task<IEnumerable<ExtraService>> GetByBookingIdAsync(Guid bookingId);
        Task<decimal> GetTotalPriceByBookingIdAsync(Guid bookingId);
        Task<bool> RemoveByBookingIdAsync(Guid bookingId);
    }
}