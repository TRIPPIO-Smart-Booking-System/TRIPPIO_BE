using Trippio.Core.Domain.Entities;

namespace Trippio.Core.Services
{
    public interface ITransportTripService
    {
        Task<IEnumerable<TransportTrip>> GetAllTransportTripsAsync();
        Task<IEnumerable<TransportTrip>> GetAllTransportTripsWithTransportAsync();
        Task<TransportTrip?> GetTransportTripByIdAsync(Guid id);
        Task<TransportTrip?> GetTripWithTransportAsync(Guid id);
        Task<IEnumerable<TransportTrip>> GetTripsByTransportIdAsync(Guid transportId);
        Task<IEnumerable<TransportTrip>> GetTripsByRouteAsync(string departure, string destination);
        Task<IEnumerable<TransportTrip>> GetAvailableTripsAsync(DateTime departureDate);
        Task<TransportTrip> CreateTransportTripAsync(TransportTrip transportTrip);
        Task<TransportTrip?> UpdateTransportTripAsync(Guid id, TransportTrip transportTrip);
        Task<bool> DeleteTransportTripAsync(Guid id);
    }
}
