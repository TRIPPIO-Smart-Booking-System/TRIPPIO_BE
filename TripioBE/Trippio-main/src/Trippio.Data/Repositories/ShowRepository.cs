using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class ShowRepository : RepositoryBase<Show, Guid>, IShowRepository
    {
        public ShowRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Show>> GetShowsByCityAsync(string city)
        {
            return await _context.Shows
                .Where(s => s.City.ToLower() == city.ToLower())
                .ToListAsync();
        }

        public async Task<IEnumerable<Show>> GetUpcomingShowsAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _context.Shows
                .Where(s => s.StartDate >= currentDate)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Show>> GetShowsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Shows
                .Where(s => s.StartDate >= startDate && s.EndDate <= endDate)
                .ToListAsync();
        }
    }
}
