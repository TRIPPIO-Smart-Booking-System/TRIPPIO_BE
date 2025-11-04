using System.ComponentModel.DataAnnotations;

namespace Trippio.Core.Models.Room
{
    public class CreateRoomRequest
    {
        [Required]
        public Guid HotelId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RoomType { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PricePerNight { get; set; }

        [Required]
        [Range(1, 100)]
        public int Capacity { get; set; }

        [Range(0, int.MaxValue)]
        public int AvailableRooms { get; set; }
    }

    public class UpdateRoomRequest
    {
        [Required]
        public Guid HotelId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RoomType { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PricePerNight { get; set; }

        [Required]
        [Range(1, 100)]
        public int Capacity { get; set; }

        [Range(0, int.MaxValue)]
        public int AvailableRooms { get; set; }
    }

    public class RoomDto
    {
        public Guid Id { get; set; }
        public Guid HotelId { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public int AvailableRooms { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Optional: Include hotel info if needed
        public string? HotelName { get; set; }
    }
}
