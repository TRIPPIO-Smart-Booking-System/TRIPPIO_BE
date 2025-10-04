using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class RoomRepository : RepositoryBase<Room, Guid>, IRoomRepository
    {
        public RoomRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Room>> GetRoomsByHotelIdAsync(Guid hotelId)
        {
            return await _context.Rooms
                .Where(r => r.HotelId == hotelId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(Guid hotelId)
        {
            return await _context.Rooms
                .Where(r => r.HotelId == hotelId && r.AvailableRooms > 0)
                .ToListAsync();
        }

        public async Task<Room?> GetRoomWithHotelAsync(Guid id)
        {
            return await _context.Rooms
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
