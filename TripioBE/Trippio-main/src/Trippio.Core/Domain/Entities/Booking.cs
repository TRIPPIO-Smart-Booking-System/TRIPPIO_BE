using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Trippio.Core.Domain.Identity;

namespace Trippio.Core.Domain.Entities
{   
    [Table("Bookings")]
    public class Booking
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string BookingType { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Status")]     
        public BookingStatus Status { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        public virtual ICollection<AccommodationBookingDetail> AccommodationBookingDetails { get; set; } = new List<AccommodationBookingDetail>();
        public virtual ICollection<TransportBookingDetail> TransportBookingDetails { get; set; } = new List<TransportBookingDetail>();
        public virtual ICollection<EntertainmentBookingDetail> EntertainmentBookingDetails { get; set; } = new List<EntertainmentBookingDetail>();
        public virtual ICollection<ExtraService> ExtraServices { get; set; } = new List<ExtraService>();
        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}