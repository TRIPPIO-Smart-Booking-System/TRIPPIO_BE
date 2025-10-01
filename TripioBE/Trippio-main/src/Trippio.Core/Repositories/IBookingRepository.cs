using Trippio.Core.Domain.Entities;
using Trippio.Core.Models;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface IBookingRepository : IRepository<Booking, Guid>
    {
        Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Booking>> GetByTypeAsync(string bookingType);
        Task<Booking?> GetWithDetailsAsync(Guid id);
        Task<PageResult<Booking>> GetPagedByUserIdAsync(Guid userId, int pageIndex, int pageSize);
        Task<IEnumerable<Booking>> GetByDateRangeAsync(DateTime from, DateTime to);
    }

    public interface IAccommodationBookingDetailRepository : IRepository<AccommodationBookingDetail, Guid>
    {
        Task<IEnumerable<AccommodationBookingDetail>> GetByBookingIdAsync(Guid bookingId);
        Task<IEnumerable<AccommodationBookingDetail>> GetByHotelIdAsync(Guid hotelId);
    }

    public interface ITransportBookingDetailRepository : IRepository<TransportBookingDetail, Guid>
    {
        Task<IEnumerable<TransportBookingDetail>> GetByBookingIdAsync(Guid bookingId);
        Task<IEnumerable<TransportBookingDetail>> GetByTicketIdAsync(Guid ticketId);
    }

    public interface IEntertainmentBookingDetailRepository : IRepository<EntertainmentBookingDetail, Guid>
    {
        Task<IEnumerable<EntertainmentBookingDetail>> GetByBookingIdAsync(Guid bookingId);
        Task<IEnumerable<EntertainmentBookingDetail>> GetByShowIdAsync(Guid showId);
    }
}