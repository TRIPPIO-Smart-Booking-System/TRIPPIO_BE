// File: Trippio.Data/Service/BookingService.cs
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Models.Booking;
using Trippio.Core.Models.Common;
using Trippio.Core.Repositories;
using Trippio.Core.SeedWorks;
using Trippio.Core.Services;

namespace Trippio.Data.Service
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public BookingService(IBookingRepository bookingRepo, IUnitOfWork uow, IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<BaseResponse<IEnumerable<BookingDto>>> GetByUserIdAsync(Guid userId)
        {
            var data = await _bookingRepo.GetByUserIdAsync(userId);
            var mapped = _mapper.Map<IEnumerable<BookingDto>>(data);
            return BaseResponse<IEnumerable<BookingDto>>.Success(mapped);
        }

        public async Task<BaseResponse<BookingDto>> GetByIdAsync(Guid id)
        {
            var entity = await _bookingRepo.GetWithDetailsAsync(id);
            if (entity == null)
                return BaseResponse<BookingDto>.NotFound("Booking not found");

            return BaseResponse<BookingDto>.Success(_mapper.Map<BookingDto>(entity));
        }

        public async Task<BaseResponse<IEnumerable<BookingDto>>> GetByStatusAsync(string status)
        {
            var data = await _bookingRepo.GetByStatusAsync(status);
            var mapped = _mapper.Map<IEnumerable<BookingDto>>(data);
            return BaseResponse<IEnumerable<BookingDto>>.Success(mapped);
        }

        public async Task<BaseResponse<IEnumerable<BookingDto>>> GetUpcomingBookingsAsync(Guid userId)
        {
            var data = await _bookingRepo.GetUpcomingBookingsAsync(userId);
            var mapped = _mapper.Map<IEnumerable<BookingDto>>(data);
            return BaseResponse<IEnumerable<BookingDto>>.Success(mapped);
        }

        public async Task<BaseResponse<BookingDto>> UpdateStatusAsync(Guid id, string status)
        {
            var entity = await _bookingRepo.GetByIdAsync(id);
            if (entity == null)
                return BaseResponse<BookingDto>.NotFound("Booking not found");

            entity.Status = status;
            entity.ModifiedDate = DateTime.UtcNow;
            await _uow.CompleteAsync();

            return BaseResponse<BookingDto>.Success(_mapper.Map<BookingDto>(entity), "Status updated");
        }

        public async Task<BaseResponse<bool>> CancelBookingAsync(Guid id, Guid userId)
        {
            var entity = await _bookingRepo.GetByIdAsync(id);
            if (entity == null)
                return BaseResponse<bool>.NotFound("Booking not found");

            if (entity.UserId != userId)
                return BaseResponse<bool>.Error("You cannot cancel someone else's booking", 403);

            if (entity.Status == "Cancelled")
                return BaseResponse<bool>.Success(true, "Booking already cancelled");

            if (entity.Status != "Pending")
                return BaseResponse<bool>.Error("Only pending bookings can be cancelled", 409);

            entity.Status = "Cancelled";
            entity.ModifiedDate = DateTime.UtcNow;
            await _uow.CompleteAsync();

            return BaseResponse<bool>.Success(true, "Booking cancelled");
        }

        public async Task<BaseResponse<decimal>> GetTotalBookingValueAsync(DateTime from, DateTime to)
        {
            if (to < from)
                return BaseResponse<decimal>.Error("End date must be after start date");

            var total = await _bookingRepo.GetTotalBookingValueAsync(from, to);
            return BaseResponse<decimal>.Success(total);
        }
    }
}
