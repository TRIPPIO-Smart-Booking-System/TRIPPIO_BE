using Trippio.Core.Domain.Entities;
using Trippio.Core.Models.Room;

namespace Trippio.Core.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetAllRoomsAsync();
        Task<Room?> GetRoomByIdAsync(Guid id);
        Task<Room?> GetRoomWithHotelAsync(Guid id);
        Task<IEnumerable<Room>> GetRoomsByHotelIdAsync(Guid hotelId);
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(Guid hotelId);
        Task<Room> CreateRoomAsync(CreateRoomRequest request);
        Task<Room?> UpdateRoomAsync(Guid id, UpdateRoomRequest request);
        Task<bool> DeleteRoomAsync(Guid id);
    }
}
