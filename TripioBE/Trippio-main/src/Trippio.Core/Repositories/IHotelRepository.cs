using Trippio.Core.Domain.Entities;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface IHotelRepository : IRepository<Hotel, Guid>
    {
        Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city);
        Task<IEnumerable<Hotel>> GetHotelsByStarsAsync(int stars);
        Task<Hotel?> GetHotelWithRoomsAsync(Guid id);
    }
}
