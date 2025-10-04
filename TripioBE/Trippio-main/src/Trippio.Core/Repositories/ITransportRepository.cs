using Trippio.Core.Domain.Entities;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface ITransportRepository : IRepository<Transport, Guid>
    {
        Task<IEnumerable<Transport>> GetTransportsByTypeAsync(string transportType);
        Task<Transport?> GetTransportWithTripsAsync(Guid id);
    }
}
