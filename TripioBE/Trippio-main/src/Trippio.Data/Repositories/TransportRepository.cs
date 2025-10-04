using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class TransportRepository : RepositoryBase<Transport, Guid>, ITransportRepository
    {
        public TransportRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Transport>> GetTransportsByTypeAsync(string transportType)
        {
            return await _context.Transports
                .Where(t => t.TransportType.ToLower() == transportType.ToLower())
                .ToListAsync();
        }

        public async Task<Transport?> GetTransportWithTripsAsync(Guid id)
        {
            return await _context.Transports
                .Include(t => t.TransportTrips)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
