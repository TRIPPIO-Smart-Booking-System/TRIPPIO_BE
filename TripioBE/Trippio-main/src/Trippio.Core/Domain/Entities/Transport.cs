using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trippio.Core.Domain.Entities
{
    [Table("Transports")]
    public class Transport
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public required string TransportType { get; set; } // Plane, Train, Bus

        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        public virtual ICollection<TransportTrip> TransportTrips { get; set; } = new List<TransportTrip>();
    }
}