using Trippio.Core.Domain.Entities;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface IShowRepository : IRepository<Show, Guid>
    {
        Task<IEnumerable<Show>> GetShowsByCityAsync(string city);
        Task<IEnumerable<Show>> GetUpcomingShowsAsync();
        Task<IEnumerable<Show>> GetShowsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
