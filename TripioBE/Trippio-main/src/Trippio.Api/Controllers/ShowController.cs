using Microsoft.AspNetCore.Mvc;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Services;

namespace Trippio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowController : ControllerBase
    {
        private readonly IShowService _showService;
        private readonly ILogger<ShowController> _logger;

        public ShowController(IShowService showService, ILogger<ShowController> logger)
        {
            _showService = showService;
            _logger = logger;
        }

        /// <summary>
        /// Get all shows
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Show>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var shows = await _showService.GetAllShowsAsync();
                return Ok(shows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all shows");
                return StatusCode(500, new { message = "An error occurred while retrieving shows" });
            }
        }

        /// <summary>
        /// Get show by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Show), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var show = await _showService.GetShowByIdAsync(id);
                if (show == null)
                    return NotFound(new { message = $"Show with ID {id} not found" });

                return Ok(show);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting show by ID: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the show" });
            }
        }

        /// <summary>
        /// Get shows by city
        /// </summary>
        [HttpGet("city/{city}")]
        [ProducesResponseType(typeof(IEnumerable<Show>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCity(string city)
        {
            try
            {
                var shows = await _showService.GetShowsByCityAsync(city);
                return Ok(shows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shows by city: {City}", city);
                return StatusCode(500, new { message = "An error occurred while retrieving shows" });
            }
        }

        /// <summary>
        /// Get upcoming shows
        /// </summary>
        [HttpGet("upcoming")]
        [ProducesResponseType(typeof(IEnumerable<Show>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUpcoming()
        {
            try
            {
                var shows = await _showService.GetUpcomingShowsAsync();
                return Ok(shows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming shows");
                return StatusCode(500, new { message = "An error occurred while retrieving upcoming shows" });
            }
        }

        /// <summary>
        /// Get shows by date range
        /// </summary>
        [HttpGet("daterange")]
        [ProducesResponseType(typeof(IEnumerable<Show>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                    return BadRequest(new { message = "Start date must be before or equal to end date" });

                var shows = await _showService.GetShowsByDateRangeAsync(startDate, endDate);
                return Ok(shows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shows by date range: {StartDate} - {EndDate}", startDate, endDate);
                return StatusCode(500, new { message = "An error occurred while retrieving shows" });
            }
        }

        /// <summary>
        /// Create a new show
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Show), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Show show)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdShow = await _showService.CreateShowAsync(show);
                return CreatedAtAction(nameof(GetById), new { id = createdShow.Id }, createdShow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating show");
                return StatusCode(500, new { message = "An error occurred while creating the show" });
            }
        }

        /// <summary>
        /// Update an existing show
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(Show), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] Show show)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedShow = await _showService.UpdateShowAsync(id, show);
                if (updatedShow == null)
                    return NotFound(new { message = $"Show with ID {id} not found" });

                return Ok(updatedShow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating show: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while updating the show" });
            }
        }

        /// <summary>
        /// Delete a show
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _showService.DeleteShowAsync(id);
                if (!result)
                    return NotFound(new { message = $"Show with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting show: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the show" });
            }
        }
    }
}
