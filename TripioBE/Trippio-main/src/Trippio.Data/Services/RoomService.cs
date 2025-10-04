using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Core.Services;
using Trippio.Core.SeedWorks;

namespace Trippio.Data.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RoomService(IRoomRepository roomRepository, IUnitOfWork unitOfWork)
        {
            _roomRepository = roomRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            return await _roomRepository.GetAllAsync();
        }

        public async Task<Room?> GetRoomByIdAsync(Guid id)
        {
            return await _roomRepository.GetByIdAsync(id);
        }

        public async Task<Room?> GetRoomWithHotelAsync(Guid id)
        {
            return await _roomRepository.GetRoomWithHotelAsync(id);
        }

        public async Task<IEnumerable<Room>> GetRoomsByHotelIdAsync(Guid hotelId)
        {
            return await _roomRepository.GetRoomsByHotelIdAsync(hotelId);
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(Guid hotelId)
        {
            return await _roomRepository.GetAvailableRoomsAsync(hotelId);
        }

        public async Task<Room> CreateRoomAsync(Room room)
        {
            room.DateCreated = DateTime.UtcNow;
            await _roomRepository.Add(room);
            await _unitOfWork.CompleteAsync();
            return room;
        }

        public async Task<Room?> UpdateRoomAsync(Guid id, Room room)
        {
            var existingRoom = await _roomRepository.GetByIdAsync(id);
            if (existingRoom == null)
                return null;

            existingRoom.HotelId = room.HotelId;
            existingRoom.RoomType = room.RoomType;
            existingRoom.PricePerNight = room.PricePerNight;
            existingRoom.Capacity = room.Capacity;
            existingRoom.AvailableRooms = room.AvailableRooms;
            existingRoom.ModifiedDate = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();
            return existingRoom;
        }

        public async Task<bool> DeleteRoomAsync(Guid id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
                return false;

            _roomRepository.Remove(room);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
