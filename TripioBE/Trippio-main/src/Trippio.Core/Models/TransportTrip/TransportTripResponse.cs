namespace Trippio.Core.Models.TransportTrip
{
    /// <summary>
    /// Response model for transport trip
    /// </summary>
    public class TransportTripResponse
    {
        public Guid Id { get; set; }
        public Guid TransportId { get; set; }
        public string Departure { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }
        
        // Transport details (without circular reference)
        public string? TransportName { get; set; }
        public string? TransportType { get; set; }
    }
}