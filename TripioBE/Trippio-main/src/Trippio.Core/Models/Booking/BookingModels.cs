using System.ComponentModel.DataAnnotations;

namespace Trippio.Core.Models.Booking
{
    public class CreateBookingRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string BookingType { get; set; } // "Accommodation", "Transport", "Entertainment"

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Status { get; set; } = "Pending";
    }

    public class CreateAccommodationBookingRequest
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public Guid HotelId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [MaxLength(100)]
        public required string RoomType { get; set; }

        [Required]
        public int GuestCount { get; set; }
    }

    public class CreateTransportBookingRequest
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public Guid TicketId { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Required]
        [MaxLength(50)]
        public required string SeatNumber { get; set; }
    }

    public class CreateEntertainmentBookingRequest
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public Guid ShowId { get; set; }

        [Required]
        public DateTime ShowDate { get; set; }

        [Required]
        [MaxLength(50)]
        public required string SeatNumber { get; set; }
    }

    public class BookingDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string BookingType { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }
}