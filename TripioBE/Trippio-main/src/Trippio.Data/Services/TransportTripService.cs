using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Core.Services;
using Trippio.Core.SeedWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trippio.Data;

namespace Trippio.Data.Services
{
    public class TransportTripService : ITransportTripService
    {
        private readonly ITransportTripRepository _transportTripRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TrippioDbContext _context;
        private readonly ILogger<TransportTripService> _logger;

        public TransportTripService(
            ITransportTripRepository transportTripRepository, 
            IUnitOfWork unitOfWork,
            TrippioDbContext context,
            ILogger<TransportTripService> logger)
        {
            _transportTripRepository = transportTripRepository;
            _unitOfWork = unitOfWork;
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TransportTrip>> GetAllTransportTripsAsync()
        {
            return await _transportTripRepository.GetAllAsync();
        }

        public async Task<IEnumerable<TransportTrip>> GetAllTransportTripsWithTransportAsync()
        {
            return await _transportTripRepository.GetAllWithTransportAsync();
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
            try
            {
                // Validate if Transport exists and get transport details
                var transport = await _context.Transports.FirstOrDefaultAsync(t => t.Id == transportTrip.TransportId);
                if (transport == null)
                {
                    throw new InvalidOperationException($"Transport with ID {transportTrip.TransportId} does not exist.");
                }

                // Clear any navigation properties to avoid EF tracking issues
                transportTrip.Transport = null!;
                
                // Set audit fields
                transportTrip.DateCreated = DateTime.UtcNow;
                transportTrip.ModifiedDate = null;

                // Add to repository
                await _transportTripRepository.Add(transportTrip);
                
                // Save changes
                await _unitOfWork.CompleteAsync();

                // Load the created trip with transport details
                var createdTrip = await _context.TransportTrips
                    .Include(tt => tt.Transport)
                    .FirstOrDefaultAsync(tt => tt.Id == transportTrip.Id);

                return createdTrip ?? transportTrip;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transport trip: {TransportId}, {Departure} -> {Destination}", 
                    transportTrip.TransportId, transportTrip.Departure, transportTrip.Destination);
                throw;
            }
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
