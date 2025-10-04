using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Core.Services;
using Trippio.Core.SeedWorks;

namespace Trippio.Data.Services
{
    public class ShowService : IShowService
    {
        private readonly IShowRepository _showRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ShowService(IShowRepository showRepository, IUnitOfWork unitOfWork)
        {
            _showRepository = showRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Show>> GetAllShowsAsync()
        {
            return await _showRepository.GetAllAsync();
        }

        public async Task<Show?> GetShowByIdAsync(Guid id)
        {
            return await _showRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Show>> GetShowsByCityAsync(string city)
        {
            return await _showRepository.GetShowsByCityAsync(city);
        }

        public async Task<IEnumerable<Show>> GetUpcomingShowsAsync()
        {
            return await _showRepository.GetUpcomingShowsAsync();
        }

        public async Task<IEnumerable<Show>> GetShowsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _showRepository.GetShowsByDateRangeAsync(startDate, endDate);
        }

        public async Task<Show> CreateShowAsync(Show show)
        {
            show.DateCreated = DateTime.UtcNow;
            await _showRepository.Add(show);
            await _unitOfWork.CompleteAsync();
            return show;
        }

        public async Task<Show?> UpdateShowAsync(Guid id, Show show)
        {
            var existingShow = await _showRepository.GetByIdAsync(id);
            if (existingShow == null)
                return null;

            existingShow.Name = show.Name;
            existingShow.Location = show.Location;
            existingShow.City = show.City;
            existingShow.StartDate = show.StartDate;
            existingShow.EndDate = show.EndDate;
            existingShow.Price = show.Price;
            existingShow.AvailableTickets = show.AvailableTickets;
            existingShow.ModifiedDate = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();
            return existingShow;
        }

        public async Task<bool> DeleteShowAsync(Guid id)
        {
            var show = await _showRepository.GetByIdAsync(id);
            if (show == null)
                return false;

            _showRepository.Remove(show);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
