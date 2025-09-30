using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trippio.Core.Domain.Entities
{
    [Table("ProductInventories")]
    public class ProductInventory
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Warehouse { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}