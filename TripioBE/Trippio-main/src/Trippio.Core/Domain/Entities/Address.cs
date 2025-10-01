using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trippio.Core.Domain.Entities
{
    [Table("Addresses")]
    public class Address
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        [MaxLength(500)]
        public required string Street { get; set; }

        [Required]
        [MaxLength(100)]
        public required string City { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;
    }
}