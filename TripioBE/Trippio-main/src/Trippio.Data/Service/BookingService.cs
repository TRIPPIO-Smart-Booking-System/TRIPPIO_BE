// File: Trippio.Data/Service/BookingService.cs
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
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

        private readonly TrippioDbContext _db;
        private const string StatusPending = "Pending";
        private const string StatusConfirmed = "Confirmed";
        private const string StatusCancelled = "Cancelled";
        public BookingService(IBookingRepository bookingRepo, IUnitOfWork uow, IMapper mapper, TrippioDbContext db)
        {
            _bookingRepo = bookingRepo;
            _uow = uow;
            _mapper = mapper;
            _db = db;                                           
        }

        public async Task<BaseResponse<IEnumerable<BookingDto>>> GetByUserIdAsync(Guid userId)
        {
            var data = await _bookingRepo.GetByUserIdAsync(userId);
            return BaseResponse<IEnumerable<BookingDto>>.Success(_mapper.Map<IEnumerable<BookingDto>>(data));
        }

        public async Task<BaseResponse<BookingDto>> GetByIdAsync(Guid id)
        {
            var entity = await _bookingRepo.GetWithDetailsAsync(id);
            if (entity is null) return BaseResponse<BookingDto>.NotFound("Booking not found");
            return BaseResponse<BookingDto>.Success(_mapper.Map<BookingDto>(entity));
        }

        public async Task<BaseResponse<IEnumerable<BookingDto>>> GetByStatusAsync(string status)
        {
            var data = await _bookingRepo.GetByStatusAsync(status);
            return BaseResponse<IEnumerable<BookingDto>>.Success(_mapper.Map<IEnumerable<BookingDto>>(data));
        }

        public async Task<BaseResponse<IEnumerable<BookingDto>>> GetUpcomingBookingsAsync(Guid userId)
        {
            var data = await _bookingRepo.GetUpcomingBookingsAsync(userId);
            return BaseResponse<IEnumerable<BookingDto>>.Success(_mapper.Map<IEnumerable<BookingDto>>(data));
        }

        public async Task<BaseResponse<BookingDto>> UpdateStatusAsync(Guid id, string status)
        {
            var entity = await _bookingRepo.GetByIdAsync(id);
            if (entity is null) return BaseResponse<BookingDto>.NotFound("Booking not found");

            var normalized = status?.Trim();
            if (normalized is not (StatusPending or StatusConfirmed or StatusCancelled))
                return BaseResponse<BookingDto>.Error($"Unknown status '{status}'", 400);

            if (entity.Status == StatusCancelled && normalized != StatusCancelled)
                return BaseResponse<BookingDto>.Error("Cancelled booking cannot transit to another status", 409);

            entity.Status = normalized;
            entity.ModifiedDate = DateTime.UtcNow;

            await _uow.CompleteAsync();
            return BaseResponse<BookingDto>.Success(_mapper.Map<BookingDto>(entity), "Status updated");
        }

        public async Task<BaseResponse<bool>> CancelBookingAsync(Guid id, Guid userId)
        {
            var entity = await _bookingRepo.GetByIdAsync(id);
            if (entity is null) return BaseResponse<bool>.NotFound("Booking not found");
            if (entity.UserId != userId) return BaseResponse<bool>.Error("You cannot cancel someone else's booking", 403);
            if (entity.Status == StatusCancelled) return BaseResponse<bool>.Success(true, "Booking already cancelled");
            if (entity.Status != StatusPending) return BaseResponse<bool>.Error("Only pending bookings can be cancelled", 409);

            entity.Status = StatusCancelled;
            entity.ModifiedDate = DateTime.UtcNow;
            await _uow.CompleteAsync();

            return BaseResponse<bool>.Success(true, "Booking cancelled");
        }

        public async Task<BaseResponse<decimal>> GetTotalBookingValueAsync(DateTime from, DateTime to)
        {
            if (to < from) return BaseResponse<decimal>.Error("End date must be after start date", 400);
            var total = await _bookingRepo.GetTotalBookingValueAsync(from, to);
            return BaseResponse<decimal>.Success(total);
        }

        public async Task<BaseResponse<BookingDto>> CreateAsync(CreateBookingRequest request)
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                BookingType = request.BookingType,     
                Status = StatusPending,           
                BookingDate = DateTime.UtcNow,        
                TotalAmount = 0m,                    
                DateCreated = DateTime.UtcNow
            };

            await _bookingRepo.AddAsync(booking);
            await _uow.CompleteAsync();

            return BaseResponse<BookingDto>.Success(_mapper.Map<BookingDto>(booking), "Booking created");
        }

        public async Task<BaseResponse<BookingDto>> AddAccommodationAsync(CreateAccommodationBookingRequest req)
        {
            var booking = await _bookingRepo.GetWithDetailsAsync(req.BookingId);
            if (booking is null) return BaseResponse<BookingDto>.NotFound("Booking not found");
            if (!booking.BookingType.Equals("Accommodation", StringComparison.OrdinalIgnoreCase))
                return BaseResponse<BookingDto>.Error("Booking type mismatch. Expected 'Accommodation'.", 400);
            if (booking.Status != StatusPending)
                return BaseResponse<BookingDto>.Error("Only pending bookings can be modified", 409);

            

            booking.ModifiedDate = DateTime.UtcNow;
            // booking.TotalAmount += lineTotal;

            await _uow.CompleteAsync();
            return BaseResponse<BookingDto>.Success(_mapper.Map<BookingDto>(booking), "Accommodation added");
        }

        public async Task<BaseResponse<BookingDto>> AddTransportAsync(CreateTransportBookingRequest req)
        {
            var booking = await _bookingRepo.GetWithDetailsAsync(req.UserId);
            if (booking is null) return BaseResponse<BookingDto>.NotFound("Booking not found");
            if (!booking.BookingType.Equals("Transport", StringComparison.OrdinalIgnoreCase))
                return BaseResponse<BookingDto>.Error("Booking type mismatch. Expected 'Transport'.", 400);
            if (booking.Status != StatusPending)
                return BaseResponse<BookingDto>.Error("Only pending bookings can be modified", 409);


            // decimal lineTotal = ...

            booking.ModifiedDate = DateTime.UtcNow;
            // booking.TotalAmount += lineTotal;

            await _uow.CompleteAsync();
            return BaseResponse<BookingDto>.Success(_mapper.Map<BookingDto>(booking), "Transport added");
        }

        public async Task<BaseResponse<BookingDto>> AddShowAsync(CreateShowBookingRequest req)
        {
            var booking = await _bookingRepo.GetWithDetailsAsync(req.UserId);


            if (booking is null)
                return BaseResponse<BookingDto>.NotFound("Booking not found");
            if (!booking.BookingType.Equals("Show", StringComparison.OrdinalIgnoreCase))
                return BaseResponse<BookingDto>.Error("Booking type mismatch. Expected 'Show'.", 400);

            if (!booking.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                return BaseResponse<BookingDto>.Error("Only pending bookings can be modified", 409);

            // TODO: check availability & pricing từ ShowService, rồi cộng TotalAmount
            // booking.TotalAmount += lineTotal;
            booking.ModifiedDate = DateTime.UtcNow;

            await _uow.CompleteAsync();
            return BaseResponse<BookingDto>.Success(_mapper.Map<BookingDto>(booking), "Show detail added");
        }


        public async Task<BaseResponse<BookingDto>> CreateRoomAsync(CreateRoomBookingRequest request)
        {
            if (request.CheckOutDate <= request.CheckInDate)
                return BaseResponse<BookingDto>.Error("checkOutDate must be after checkInDate", 400);

            // TODO: Validate Room exists & available, fetch pricing
            // decimal nightlyRate = await _rooms.GetNightlyRateAsync(request.RoomId, request.CheckInDate, request.CheckOutDate);
            // var nights = (request.CheckOutDate.Date - request.CheckInDate.Date).Days;
            // var total = nightlyRate * Math.Max(nights, 1);
            decimal total = 0m;

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                BookingType = "Accommodation",
                BookingDate = DateTime.UtcNow,
                Status = StatusPending,
                TotalAmount = total,
                DateCreated = DateTime.UtcNow
            };

            await _bookingRepo.AddAsync(booking);

            var detail = new AccommodationBookingDetail
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                RoomId = request.RoomId,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                GuestCount = request.GuestCount,
                DateCreated = DateTime.UtcNow
            };
            _db.Set<AccommodationBookingDetail>().Add(detail);

            await _uow.CompleteAsync();

            var dto = _mapper.Map<BookingDto>(booking);
            dto.CheckInDate = request.CheckInDate;
            dto.CheckOutDate = request.CheckOutDate;
            dto.TotalAmount = booking.TotalAmount;
         
            return BaseResponse<BookingDto>.Success(dto, "Room booking created");
        }

        public async Task<BaseResponse<BookingDto>> CreateTransportAsync(CreateTransportBookingRequest request)
        {

            decimal total = 0m;

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                BookingType = "Transport",
                BookingDate = DateTime.UtcNow,
                Status = StatusPending,
                TotalAmount = total,
                DateCreated = DateTime.UtcNow
            };

            await _bookingRepo.AddAsync(booking);

            var detail = new TransportBookingDetail
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                TripId = request.TripId,
                SeatNumber = request.SeatNumber,
                DateCreated = DateTime.UtcNow
            };
            _db.Set<TransportBookingDetail>().Add(detail);

            await _uow.CompleteAsync();

            var dto = _mapper.Map<BookingDto>(booking);
            dto.SeatNumber = request.SeatNumber;
            dto.TotalAmount = booking.TotalAmount;
           
            return BaseResponse<BookingDto>.Success(dto, "Transport booking created");
        }

        public async Task<BaseResponse<BookingDto>> CreateShowAsync(CreateShowBookingRequest request)
        {
           
            decimal total = 0m;

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                BookingType = "Entertainment",
                BookingDate = DateTime.UtcNow,
                Status = StatusPending,
                TotalAmount = total,
                DateCreated = DateTime.UtcNow
            };

            await _bookingRepo.AddAsync(booking);

            var detail = new EntertainmentBookingDetail
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                ShowId = request.ShowId,
                SeatNumber = request.SeatNumber,
                DateCreated = DateTime.UtcNow
            };
            _db.Set<EntertainmentBookingDetail>().Add(detail);

            await _uow.CompleteAsync();

            var dto = _mapper.Map<BookingDto>(booking);
            dto.ShowId = request.ShowId;
            dto.SeatNumber = request.SeatNumber;
            dto.ShowDate = request.ShowDate; 
            dto.TotalAmount = booking.TotalAmount;
            return BaseResponse<BookingDto>.Success(dto, "Show booking created");
        }
    }
}
