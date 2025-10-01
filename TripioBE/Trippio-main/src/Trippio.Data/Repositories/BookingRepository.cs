using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Core.Models;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class BookingRepository : RepositoryBase<Booking, Guid>, IBookingRepository
    {
        public BookingRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.User)
                .Include(b => b.AccommodationBookingDetails)
                .Include(b => b.TransportBookingDetails)
                .Include(b => b.EntertainmentBookingDetails)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<PageResult<Booking>> GetPagedByUserIdAsync(Guid userId, int pageIndex, int pageSize)
        {
            var query = _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.User)
                .Include(b => b.AccommodationBookingDetails)
                .Include(b => b.TransportBookingDetails)
                .Include(b => b.EntertainmentBookingDetails);

            var totalItems = await query.CountAsync();
            var items = await query.OrderByDescending(b => b.BookingDate)
                                  .Skip((pageIndex - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            return new PageResult<Booking>
            {
                Results = items,
                RowCount = totalItems,
                CurrentPage = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<Booking>> GetByStatusAsync(string status)
        {
            return await _context.Bookings
                .Where(b => b.Status == status)
                .Include(b => b.User)
                .Include(b => b.AccommodationBookingDetails)
                .Include(b => b.TransportBookingDetails)
                .Include(b => b.EntertainmentBookingDetails)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _context.Bookings
                .Where(b => b.BookingDate >= from && b.BookingDate <= to)
                .Include(b => b.User)
                .Include(b => b.AccommodationBookingDetails)
                .Include(b => b.TransportBookingDetails)
                .Include(b => b.EntertainmentBookingDetails)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(Guid userId)
        {
            var today = DateTime.Today;
            return await _context.Bookings
                .Where(b => b.UserId == userId && b.BookingDate >= today)
                .Include(b => b.User)
                .Include(b => b.AccommodationBookingDetails)
                .Include(b => b.TransportBookingDetails)
                .Include(b => b.EntertainmentBookingDetails)
                .OrderBy(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<Booking?> GetWithDetailsAsync(Guid bookingId)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.AccommodationBookingDetails)
                .Include(b => b.TransportBookingDetails)
                .Include(b => b.EntertainmentBookingDetails)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task<decimal> GetTotalBookingValueAsync(DateTime from, DateTime to)
        {
            return await _context.Bookings
                .Where(b => b.BookingDate >= from && b.BookingDate <= to && b.Status == "Confirmed")
                .SumAsync(b => b.TotalAmount);
        }

        public async Task<IEnumerable<Booking>> GetByTypeAsync(string bookingType)
        {
            return await _context.Bookings
                .Where(b => b.BookingType == bookingType)
                .Include(b => b.User)
                .Include(b => b.AccommodationBookingDetails)
                .Include(b => b.TransportBookingDetails)
                .Include(b => b.EntertainmentBookingDetails)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }
    }
}
