using Trippio.Core.Domain.Entities;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface ITransportTripRepository : IRepository<TransportTrip, Guid>
    {
        Task<IEnumerable<TransportTrip>> GetTripsByTransportIdAsync(Guid transportId);
        Task<IEnumerable<TransportTrip>> GetTripsByRouteAsync(string departure, string destination);
        Task<IEnumerable<TransportTrip>> GetAvailableTripsAsync(DateTime departureDate);
        Task<TransportTrip?> GetTripWithTransportAsync(Guid id);
    }
}
