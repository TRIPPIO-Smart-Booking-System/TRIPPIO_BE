using Trippio.Core.Domain.Entities;

namespace Trippio.Core.Services
{
    public interface ITransportService
    {
        Task<IEnumerable<Transport>> GetAllTransportsAsync();
        Task<Transport?> GetTransportByIdAsync(Guid id);
        Task<Transport?> GetTransportWithTripsAsync(Guid id);
        Task<IEnumerable<Transport>> GetTransportsByTypeAsync(string transportType);
        Task<Transport> CreateTransportAsync(Transport transport);
        Task<Transport?> UpdateTransportAsync(Guid id, Transport transport);
        Task<bool> DeleteTransportAsync(Guid id);
    }
}
