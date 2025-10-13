using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class TransportTripRepository : RepositoryBase<TransportTrip, Guid>, ITransportTripRepository
    {
        public TransportTripRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TransportTrip>> GetTripsByTransportIdAsync(Guid transportId)
        {
            return await _context.TransportTrips
                .Where(tt => tt.TransportId == transportId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TransportTrip>> GetTripsByRouteAsync(string departure, string destination)
        {
            return await _context.TransportTrips
                .Where(tt => tt.Departure.ToLower() == departure.ToLower() 
                          && tt.Destination.ToLower() == destination.ToLower())
                .ToListAsync();
        }

        public async Task<IEnumerable<TransportTrip>> GetAvailableTripsAsync(DateTime departureDate)
        {
            return await _context.TransportTrips
                .Where(tt => tt.DepartureTime.Date == departureDate.Date && tt.AvailableSeats > 0)
                .ToListAsync();
        }

        public async Task<TransportTrip?> GetTripWithTransportAsync(Guid id)
        {
            return await _context.TransportTrips
                .Include(tt => tt.Transport)
                .FirstOrDefaultAsync(tt => tt.Id == id);
        }

        public async Task<IEnumerable<TransportTrip>> GetAllWithTransportAsync()
        {
            return await _context.TransportTrips
                .Include(tt => tt.Transport)
                .ToListAsync();
        }
    }
}
