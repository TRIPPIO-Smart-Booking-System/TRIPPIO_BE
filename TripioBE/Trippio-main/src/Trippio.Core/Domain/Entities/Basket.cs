using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Trippio.Core.Domain.Identity;

namespace Trippio.Core.Domain.Entities
{
    // Note: This entity is typically used for Redis cache, not SQL database
    // But we define it here for consistency with the schema
    [Table("Baskets")]
    public class Basket
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        public required string BasketData { get; set; } // JSON data

        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }
}