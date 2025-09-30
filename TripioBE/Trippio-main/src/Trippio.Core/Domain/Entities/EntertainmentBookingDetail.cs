using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trippio.Core.Domain.Entities
{
    [Table("EntertainmentBookingDetails")]
    public class EntertainmentBookingDetail
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public Guid ShowId { get; set; }

        [Required]
        public DateTime ShowDate { get; set; }

        [Required]
        [MaxLength(50)]
        public required string SeatNumber { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;
    }
}