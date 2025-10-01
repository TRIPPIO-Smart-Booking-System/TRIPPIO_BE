using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trippio.Core.Domain.Entities
{
    [Table("TransportTrips")]
    public class TransportTrip
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TransportId { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Departure { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Destination { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int AvailableSeats { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("TransportId")]
        public virtual Transport Transport { get; set; } = null!;
    }
}