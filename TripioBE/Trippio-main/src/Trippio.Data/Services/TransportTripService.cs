using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Core.Services;
using Trippio.Core.SeedWorks;

namespace Trippio.Data.Services
{
    public class TransportTripService : ITransportTripService
    {
        private readonly ITransportTripRepository _transportTripRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TransportTripService(ITransportTripRepository transportTripRepository, IUnitOfWork unitOfWork)
        {
            _transportTripRepository = transportTripRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TransportTrip>> GetAllTransportTripsAsync()
        {
            return await _transportTripRepository.GetAllAsync();
        }

        public async Task<TransportTrip?> GetTransportTripByIdAsync(Guid id)
        {
            return await _transportTripRepository.GetByIdAsync(id);
        }

        public async Task<TransportTrip?> GetTripWithTransportAsync(Guid id)
        {
            return await _transportTripRepository.GetTripWithTransportAsync(id);
        }

        public async Task<IEnumerable<TransportTrip>> GetTripsByTransportIdAsync(Guid transportId)
        {
            return await _transportTripRepository.GetTripsByTransportIdAsync(transportId);
        }

        public async Task<IEnumerable<TransportTrip>> GetTripsByRouteAsync(string departure, string destination)
        {
            return await _transportTripRepository.GetTripsByRouteAsync(departure, destination);
        }

        public async Task<IEnumerable<TransportTrip>> GetAvailableTripsAsync(DateTime departureDate)
        {
            return await _transportTripRepository.GetAvailableTripsAsync(departureDate);
        }

        public async Task<TransportTrip> CreateTransportTripAsync(TransportTrip transportTrip)
        {
            transportTrip.DateCreated = DateTime.UtcNow;
            await _transportTripRepository.Add(transportTrip);
            await _unitOfWork.CompleteAsync();
            return transportTrip;
        }

        public async Task<TransportTrip?> UpdateTransportTripAsync(Guid id, TransportTrip transportTrip)
        {
            var existingTrip = await _transportTripRepository.GetByIdAsync(id);
            if (existingTrip == null)
                return null;

            existingTrip.TransportId = transportTrip.TransportId;
            existingTrip.Departure = transportTrip.Departure;
            existingTrip.Destination = transportTrip.Destination;
            existingTrip.DepartureTime = transportTrip.DepartureTime;
            existingTrip.ArrivalTime = transportTrip.ArrivalTime;
            existingTrip.Price = transportTrip.Price;
            existingTrip.AvailableSeats = transportTrip.AvailableSeats;
            existingTrip.ModifiedDate = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();
            return existingTrip;
        }

        public async Task<bool> DeleteTransportTripAsync(Guid id)
        {
            var trip = await _transportTripRepository.GetByIdAsync(id);
            if (trip == null)
                return false;

            _transportTripRepository.Remove(trip);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
