using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trippio.Core.Domain.Entities
{
    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;
    }
}