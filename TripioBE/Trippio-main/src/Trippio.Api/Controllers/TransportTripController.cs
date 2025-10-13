using Microsoft.AspNetCore.Mvc;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Services;
using Trippio.Core.Models.TransportTrip;

namespace Trippio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransportTripController : ControllerBase
    {
        private readonly ITransportTripService _transportTripService;
        private readonly ILogger<TransportTripController> _logger;

        public TransportTripController(ITransportTripService transportTripService, ILogger<TransportTripController> logger)
        {
            _transportTripService = transportTripService;
            _logger = logger;
        }

        /// <summary>
        /// Get all transport trips
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TransportTripResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var trips = await _transportTripService.GetAllTransportTripsWithTransportAsync();
                var response = trips.Select(trip => new TransportTripResponse
                {
                    Id = trip.Id,
                    TransportId = trip.TransportId,
                    Departure = trip.Departure,
                    Destination = trip.Destination,
                    DepartureTime = trip.DepartureTime,
                    ArrivalTime = trip.ArrivalTime,
                    Price = trip.Price,
                    AvailableSeats = trip.AvailableSeats,
                    DateCreated = trip.DateCreated,
                    ModifiedDate = trip.ModifiedDate,
                    TransportName = trip.Transport?.Name,
                    TransportType = trip.Transport?.TransportType
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all transport trips");
                return StatusCode(500, new { message = "An error occurred while retrieving transport trips" });
            }
        }

        /// <summary>
        /// Get transport trip by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TransportTripResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var trip = await _transportTripService.GetTripWithTransportAsync(id);
                if (trip == null)
                    return NotFound(new { message = $"Transport trip with ID {id} not found" });

                var response = new TransportTripResponse
                {
                    Id = trip.Id,
                    TransportId = trip.TransportId,
                    Departure = trip.Departure,
                    Destination = trip.Destination,
                    DepartureTime = trip.DepartureTime,
                    ArrivalTime = trip.ArrivalTime,
                    Price = trip.Price,
                    AvailableSeats = trip.AvailableSeats,
                    DateCreated = trip.DateCreated,
                    ModifiedDate = trip.ModifiedDate,
                    TransportName = trip.Transport?.Name,
                    TransportType = trip.Transport?.TransportType
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transport trip by ID: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the transport trip" });
            }
        }

        /// <summary>
        /// Get transport trip with transport details
        /// </summary>
        [HttpGet("{id:guid}/transport")]
        [ProducesResponseType(typeof(TransportTrip), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWithTransport(Guid id)
        {
            try
            {
                var trip = await _transportTripService.GetTripWithTransportAsync(id);
                if (trip == null)
                    return NotFound(new { message = $"Transport trip with ID {id} not found" });

                return Ok(trip);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transport trip with transport: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the transport trip" });
            }
        }

        /// <summary>
        /// Get trips by transport ID
        /// </summary>
        [HttpGet("transport/{transportId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<TransportTrip>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByTransportId(Guid transportId)
        {
            try
            {
                var trips = await _transportTripService.GetTripsByTransportIdAsync(transportId);
                return Ok(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trips by transport ID: {TransportId}", transportId);
                return StatusCode(500, new { message = "An error occurred while retrieving transport trips" });
            }
        }

        /// <summary>
        /// Get trips by route
        /// </summary>
        [HttpGet("route")]
        [ProducesResponseType(typeof(IEnumerable<TransportTrip>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByRoute([FromQuery] string departure, [FromQuery] string destination)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(departure) || string.IsNullOrWhiteSpace(destination))
                    return BadRequest(new { message = "Departure and destination are required" });

                var trips = await _transportTripService.GetTripsByRouteAsync(departure, destination);
                return Ok(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trips by route: {Departure} -> {Destination}", departure, destination);
                return StatusCode(500, new { message = "An error occurred while retrieving transport trips" });
            }
        }

        /// <summary>
        /// Get available trips by departure date
        /// </summary>
        [HttpGet("available")]
        [ProducesResponseType(typeof(IEnumerable<TransportTrip>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableTrips([FromQuery] DateTime departureDate)
        {
            try
            {
                var trips = await _transportTripService.GetAvailableTripsAsync(departureDate);
                return Ok(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available trips for date: {Date}", departureDate);
                return StatusCode(500, new { message = "An error occurred while retrieving available trips" });
            }
        }

        /// <summary>
        /// Create a new transport trip
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TransportTripResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateTransportTripRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Map request to entity
                var transportTrip = new TransportTrip
                {
                    TransportId = request.TransportId,
                    Departure = request.Departure,
                    Destination = request.Destination,
                    DepartureTime = request.DepartureTime,
                    ArrivalTime = request.ArrivalTime,
                    Price = request.Price,
                    AvailableSeats = request.AvailableSeats
                };

                var createdTrip = await _transportTripService.CreateTransportTripAsync(transportTrip);
                
                // Map to response model with transport details
                var response = new TransportTripResponse
                {
                    Id = createdTrip.Id,
                    TransportId = createdTrip.TransportId,
                    Departure = createdTrip.Departure,
                    Destination = createdTrip.Destination,
                    DepartureTime = createdTrip.DepartureTime,
                    ArrivalTime = createdTrip.ArrivalTime,
                    Price = createdTrip.Price,
                    AvailableSeats = createdTrip.AvailableSeats,
                    DateCreated = createdTrip.DateCreated,
                    ModifiedDate = createdTrip.ModifiedDate,
                    TransportName = createdTrip.Transport?.Name,
                    TransportType = createdTrip.Transport?.TransportType
                };

                return CreatedAtAction(nameof(GetById), new { id = createdTrip.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating transport trip");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transport trip");
                return StatusCode(500, new { message = "An error occurred while creating the transport trip" });
            }
        }

        /// <summary>
        /// Update an existing transport trip
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(TransportTrip), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] TransportTrip transportTrip)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedTrip = await _transportTripService.UpdateTransportTripAsync(id, transportTrip);
                if (updatedTrip == null)
                    return NotFound(new { message = $"Transport trip with ID {id} not found" });

                return Ok(updatedTrip);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transport trip: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while updating the transport trip" });
            }
        }

        /// <summary>
        /// Delete a transport trip
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _transportTripService.DeleteTransportTripAsync(id);
                if (!result)
                    return NotFound(new { message = $"Transport trip with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transport trip: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the transport trip" });
            }
        }
    }
}
