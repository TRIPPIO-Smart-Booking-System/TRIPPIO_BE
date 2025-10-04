using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Core.Services;
using Trippio.Core.SeedWorks;

namespace Trippio.Data.Services
{
    public class HotelService : IHotelService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IUnitOfWork _unitOfWork;

        public HotelService(IHotelRepository hotelRepository, IUnitOfWork unitOfWork)
        {
            _hotelRepository = hotelRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Hotel>> GetAllHotelsAsync()
        {
            return await _hotelRepository.GetAllAsync();
        }

        public async Task<Hotel?> GetHotelByIdAsync(Guid id)
        {
            return await _hotelRepository.GetByIdAsync(id);
        }

        public async Task<Hotel?> GetHotelWithRoomsAsync(Guid id)
        {
            return await _hotelRepository.GetHotelWithRoomsAsync(id);
        }

        public async Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city)
        {
            return await _hotelRepository.GetHotelsByCityAsync(city);
        }

        public async Task<IEnumerable<Hotel>> GetHotelsByStarsAsync(int stars)
        {
            return await _hotelRepository.GetHotelsByStarsAsync(stars);
        }

        public async Task<Hotel> CreateHotelAsync(Hotel hotel)
        {
            hotel.DateCreated = DateTime.UtcNow;
            await _hotelRepository.Add(hotel);
            await _unitOfWork.CompleteAsync();
            return hotel;
        }

        public async Task<Hotel?> UpdateHotelAsync(Guid id, Hotel hotel)
        {
            var existingHotel = await _hotelRepository.GetByIdAsync(id);
            if (existingHotel == null)
                return null;

            existingHotel.Name = hotel.Name;
            existingHotel.Address = hotel.Address;
            existingHotel.City = hotel.City;
            existingHotel.Country = hotel.Country;
            existingHotel.Description = hotel.Description;
            existingHotel.Stars = hotel.Stars;
            existingHotel.ModifiedDate = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();
            return existingHotel;
        }

        public async Task<bool> DeleteHotelAsync(Guid id)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            if (hotel == null)
                return false;

            _hotelRepository.Remove(hotel);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
