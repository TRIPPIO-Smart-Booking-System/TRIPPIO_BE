using Trippio.Core.Domain.Entities;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface IRoomRepository : IRepository<Room, Guid>
    {
        Task<IEnumerable<Room>> GetRoomsByHotelIdAsync(Guid hotelId);
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(Guid hotelId);
        Task<Room?> GetRoomWithHotelAsync(Guid id);
    }
}
