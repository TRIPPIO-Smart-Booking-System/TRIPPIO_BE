using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trippio.Core.Domain.Entities
{
    [Table("TransportBookingDetails")]
    public class TransportBookingDetail
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public Guid TripId { get; set; }

        [Required]
        [MaxLength(50)]
        public required string SeatNumber { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;

        [ForeignKey("TripId")]
        public virtual TransportTrip TransportTrip { get; set; } = null!;
    }
}