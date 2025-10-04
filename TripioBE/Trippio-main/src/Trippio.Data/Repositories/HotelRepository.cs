using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class HotelRepository : RepositoryBase<Hotel, Guid>, IHotelRepository
    {
        public HotelRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city)
        {
            return await _context.Hotels
                .Where(h => h.City.ToLower() == city.ToLower())
                .ToListAsync();
        }

        public async Task<IEnumerable<Hotel>> GetHotelsByStarsAsync(int stars)
        {
            return await _context.Hotels
                .Where(h => h.Stars == stars)
                .ToListAsync();
        }

        public async Task<Hotel?> GetHotelWithRoomsAsync(Guid id)
        {
            return await _context.Hotels
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.Id == id);
        }
    }
}
