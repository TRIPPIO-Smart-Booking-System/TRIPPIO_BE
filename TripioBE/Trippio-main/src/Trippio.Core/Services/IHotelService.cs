using Trippio.Core.Domain.Entities;

namespace Trippio.Core.Services
{
    public interface IHotelService
    {
        Task<IEnumerable<Hotel>> GetAllHotelsAsync();
        Task<Hotel?> GetHotelByIdAsync(Guid id);
        Task<Hotel?> GetHotelWithRoomsAsync(Guid id);
        Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city);
        Task<IEnumerable<Hotel>> GetHotelsByStarsAsync(int stars);
        Task<Hotel> CreateHotelAsync(Hotel hotel);
        Task<Hotel?> UpdateHotelAsync(Guid id, Hotel hotel);
        Task<bool> DeleteHotelAsync(Guid id);
    }
}
