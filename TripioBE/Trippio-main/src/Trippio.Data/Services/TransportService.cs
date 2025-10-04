using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Core.Services;
using Trippio.Core.SeedWorks;

namespace Trippio.Data.Services
{
    public class TransportService : ITransportService
    {
        private readonly ITransportRepository _transportRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TransportService(ITransportRepository transportRepository, IUnitOfWork unitOfWork)
        {
            _transportRepository = transportRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Transport>> GetAllTransportsAsync()
        {
            return await _transportRepository.GetAllAsync();
        }

        public async Task<Transport?> GetTransportByIdAsync(Guid id)
        {
            return await _transportRepository.GetByIdAsync(id);
        }

        public async Task<Transport?> GetTransportWithTripsAsync(Guid id)
        {
            return await _transportRepository.GetTransportWithTripsAsync(id);
        }

        public async Task<IEnumerable<Transport>> GetTransportsByTypeAsync(string transportType)
        {
            return await _transportRepository.GetTransportsByTypeAsync(transportType);
        }

        public async Task<Transport> CreateTransportAsync(Transport transport)
        {
            transport.DateCreated = DateTime.UtcNow;
            await _transportRepository.Add(transport);
            await _unitOfWork.CompleteAsync();
            return transport;
        }

        public async Task<Transport?> UpdateTransportAsync(Guid id, Transport transport)
        {
            var existingTransport = await _transportRepository.GetByIdAsync(id);
            if (existingTransport == null)
                return null;

            existingTransport.TransportType = transport.TransportType;
            existingTransport.Name = transport.Name;
            existingTransport.ModifiedDate = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();
            return existingTransport;
        }

        public async Task<bool> DeleteTransportAsync(Guid id)
        {
            var transport = await _transportRepository.GetByIdAsync(id);
            if (transport == null)
                return false;

            _transportRepository.Remove(transport);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
