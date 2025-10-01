using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trippio.Core.Domain.Entities
{
    [Table("Rooms")]
    public class Room
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid HotelId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string RoomType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerNight { get; set; }

        [Required]
        public int Capacity { get; set; }

        public int AvailableRooms { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("HotelId")]
        public virtual Hotel Hotel { get; set; } = null!;
    }
}