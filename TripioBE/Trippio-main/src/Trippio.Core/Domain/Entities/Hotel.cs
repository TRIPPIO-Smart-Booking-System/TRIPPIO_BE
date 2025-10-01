using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trippio.Core.Domain.Entities
{
    [Table("Hotels")]
    public class Hotel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(500)]
        public required string Address { get; set; }

        [Required]
        [MaxLength(100)]
        public required string City { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Country { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Range(1, 5)]
        public int Stars { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}