using Trippio.Core.Models;
using Trippio.Core.Models.Common;
using Trippio.Core.Models.Booking;

namespace Trippio.Core.Services
{
    public interface IBookingService
    {
        Task<BaseResponse<IEnumerable<BookingDto>>> GetByUserIdAsync(Guid userId);
        Task<BaseResponse<BookingDto>> GetByIdAsync(Guid id);
        Task<BaseResponse<IEnumerable<BookingDto>>> GetByStatusAsync(string status);
        Task<BaseResponse<IEnumerable<BookingDto>>> GetUpcomingBookingsAsync(Guid userId);
        Task<BaseResponse<BookingDto>> UpdateStatusAsync(Guid id, string status);
        Task<BaseResponse<bool>> CancelBookingAsync(Guid id, Guid userId);
        Task<BaseResponse<decimal>> GetTotalBookingValueAsync(DateTime from, DateTime to);
    }
}