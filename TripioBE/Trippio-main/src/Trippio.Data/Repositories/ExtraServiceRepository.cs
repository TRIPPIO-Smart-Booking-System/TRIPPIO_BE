using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class ExtraServiceRepository : RepositoryBase<ExtraService, Guid>, IExtraServiceRepository
    {
        public ExtraServiceRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ExtraService>> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.ExtraServices
                .Where(es => es.BookingId == bookingId)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPriceByBookingIdAsync(Guid bookingId)
        {
            return await _context.ExtraServices
                .Where(es => es.BookingId == bookingId)
                .SumAsync(es => es.Price * es.Quantity);
        }

        public async Task<bool> RemoveByBookingIdAsync(Guid bookingId)
        {
            var extraServices = await _context.ExtraServices
                .Where(es => es.BookingId == bookingId)
                .ToListAsync();

            if (extraServices.Any())
            {
                _context.ExtraServices.RemoveRange(extraServices);
                return true;
            }

            return false;
        }
    }
}