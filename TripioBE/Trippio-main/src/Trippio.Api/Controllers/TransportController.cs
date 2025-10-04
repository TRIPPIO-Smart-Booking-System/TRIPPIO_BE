using Microsoft.AspNetCore.Mvc;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Services;

namespace Trippio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransportController : ControllerBase
    {
        private readonly ITransportService _transportService;
        private readonly ILogger<TransportController> _logger;

        public TransportController(ITransportService transportService, ILogger<TransportController> logger)
        {
            _transportService = transportService;
            _logger = logger;
        }

        /// <summary>
        /// Get all transports
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Transport>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var transports = await _transportService.GetAllTransportsAsync();
                return Ok(transports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all transports");
                return StatusCode(500, new { message = "An error occurred while retrieving transports" });
            }
        }

        /// <summary>
        /// Get transport by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Transport), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var transport = await _transportService.GetTransportByIdAsync(id);
                if (transport == null)
                    return NotFound(new { message = $"Transport with ID {id} not found" });

                return Ok(transport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transport by ID: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the transport" });
            }
        }

        /// <summary>
        /// Get transport with trips
        /// </summary>
        [HttpGet("{id:guid}/trips")]
        [ProducesResponseType(typeof(Transport), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWithTrips(Guid id)
        {
            try
            {
                var transport = await _transportService.GetTransportWithTripsAsync(id);
                if (transport == null)
                    return NotFound(new { message = $"Transport with ID {id} not found" });

                return Ok(transport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transport with trips: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the transport" });
            }
        }

        /// <summary>
        /// Get transports by type (Plane, Train, Bus)
        /// </summary>
        [HttpGet("type/{type}")]
        [ProducesResponseType(typeof(IEnumerable<Transport>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByType(string type)
        {
            try
            {
                var transports = await _transportService.GetTransportsByTypeAsync(type);
                return Ok(transports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transports by type: {Type}", type);
                return StatusCode(500, new { message = "An error occurred while retrieving transports" });
            }
        }

        /// <summary>
        /// Create a new transport
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Transport), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Transport transport)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdTransport = await _transportService.CreateTransportAsync(transport);
                return CreatedAtAction(nameof(GetById), new { id = createdTransport.Id }, createdTransport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transport");
                return StatusCode(500, new { message = "An error occurred while creating the transport" });
            }
        }

        /// <summary>
        /// Update an existing transport
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(Transport), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] Transport transport)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedTransport = await _transportService.UpdateTransportAsync(id, transport);
                if (updatedTransport == null)
                    return NotFound(new { message = $"Transport with ID {id} not found" });

                return Ok(updatedTransport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transport: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while updating the transport" });
            }
        }

        /// <summary>
        /// Delete a transport
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _transportService.DeleteTransportAsync(id);
                if (!result)
                    return NotFound(new { message = $"Transport with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transport: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the transport" });
            }
        }
    }
}
