using Trippio.Core.Domain.Entities;

namespace Trippio.Core.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetAllRoomsAsync();
        Task<Room?> GetRoomByIdAsync(Guid id);
        Task<Room?> GetRoomWithHotelAsync(Guid id);
        Task<IEnumerable<Room>> GetRoomsByHotelIdAsync(Guid hotelId);
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(Guid hotelId);
        Task<Room> CreateRoomAsync(Room room);
        Task<Room?> UpdateRoomAsync(Guid id, Room room);
        Task<bool> DeleteRoomAsync(Guid id);
    }
}
