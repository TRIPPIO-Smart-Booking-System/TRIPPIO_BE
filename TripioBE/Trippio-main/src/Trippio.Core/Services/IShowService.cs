using Trippio.Core.Domain.Entities;

namespace Trippio.Core.Services
{
    public interface IShowService
    {
        Task<IEnumerable<Show>> GetAllShowsAsync();
        Task<Show?> GetShowByIdAsync(Guid id);
        Task<IEnumerable<Show>> GetShowsByCityAsync(string city);
        Task<IEnumerable<Show>> GetUpcomingShowsAsync();
        Task<IEnumerable<Show>> GetShowsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Show> CreateShowAsync(Show show);
        Task<Show?> UpdateShowAsync(Guid id, Show show);
        Task<bool> DeleteShowAsync(Guid id);
    }
}
