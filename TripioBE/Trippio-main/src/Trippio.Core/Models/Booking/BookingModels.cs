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

        public int AvailableRooms { get; set; }
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

        public int AvailableSeats { get; set; }
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

        public int AvailableTickets { get; set; }
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
        
        // New navigation properties
        public List<ExtraServiceDto> ExtraServices { get; set; } = new();
        public List<FeedbackDto> Feedbacks { get; set; } = new();
        public List<CommentDto> Comments { get; set; } = new();
    }

    public class ExtraServiceDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class CreateExtraServiceDto
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
    }

    public class UpdateExtraServiceDto
    {
        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
    }

    public class FeedbackDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateFeedbackDto
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    public class UpdateFeedbackDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCommentDto
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        [MaxLength(2000)]
        public required string Content { get; set; }
    }

    public class UpdateCommentDto
    {
        [Required]
        [MaxLength(2000)]
        public required string Content { get; set; }
    }
}