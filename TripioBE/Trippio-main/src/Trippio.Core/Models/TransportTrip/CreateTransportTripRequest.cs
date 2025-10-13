using System.ComponentModel.DataAnnotations;

namespace Trippio.Core.Models.TransportTrip
{
    /// <summary>
    /// Request model for creating a new transport trip
    /// </summary>
    public class CreateTransportTripRequest
    {
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
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Available seats must be greater than or equal to 0")]
        public int AvailableSeats { get; set; }
    }
}