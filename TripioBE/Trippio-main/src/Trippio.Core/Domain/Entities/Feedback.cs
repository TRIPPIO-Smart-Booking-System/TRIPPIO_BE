using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trippio.Core.Domain.Entities
{
    [Table("Feedbacks")]
    public class Feedback
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BookingId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;
    }
}