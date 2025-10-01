using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trippio.Core.Domain.Entities
{
    [Table("Comments")]
    public class Comment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BookingId { get; set; }

        [Required]
        [MaxLength(2000)]
        public required string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;
    }
}