using Microsoft.AspNetCore.Mvc;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Services;

namespace Trippio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly ILogger<HotelController> _logger;

        public HotelController(IHotelService hotelService, ILogger<HotelController> logger)
        {
            _hotelService = hotelService;
            _logger = logger;
        }

        /// <summary>
        /// Get all hotels
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Hotel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var hotels = await _hotelService.GetAllHotelsAsync();
                return Ok(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all hotels");
                return StatusCode(500, new { message = "An error occurred while retrieving hotels" });
            }
        }

        /// <summary>
        /// Get hotel by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Hotel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var hotel = await _hotelService.GetHotelByIdAsync(id);
                if (hotel == null)
                    return NotFound(new { message = $"Hotel with ID {id} not found" });

                return Ok(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel by ID: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the hotel" });
            }
        }

        /// <summary>
        /// Get hotel with rooms
        /// </summary>
        [HttpGet("{id:guid}/rooms")]
        [ProducesResponseType(typeof(Hotel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWithRooms(Guid id)
        {
            try
            {
                var hotel = await _hotelService.GetHotelWithRoomsAsync(id);
                if (hotel == null)
                    return NotFound(new { message = $"Hotel with ID {id} not found" });

                return Ok(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel with rooms: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the hotel" });
            }
        }

        /// <summary>
        /// Get hotels by city
        /// </summary>
        [HttpGet("city/{city}")]
        [ProducesResponseType(typeof(IEnumerable<Hotel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCity(string city)
        {
            try
            {
                var hotels = await _hotelService.GetHotelsByCityAsync(city);
                return Ok(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotels by city: {City}", city);
                return StatusCode(500, new { message = "An error occurred while retrieving hotels" });
            }
        }

        /// <summary>
        /// Get hotels by star rating
        /// </summary>
        [HttpGet("stars/{stars:int}")]
        [ProducesResponseType(typeof(IEnumerable<Hotel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByStars(int stars)
        {
            try
            {
                var hotels = await _hotelService.GetHotelsByStarsAsync(stars);
                return Ok(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotels by stars: {Stars}", stars);
                return StatusCode(500, new { message = "An error occurred while retrieving hotels" });
            }
        }

        /// <summary>
        /// Create a new hotel
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Hotel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Hotel hotel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdHotel = await _hotelService.CreateHotelAsync(hotel);
                return CreatedAtAction(nameof(GetById), new { id = createdHotel.Id }, createdHotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hotel");
                return StatusCode(500, new { message = "An error occurred while creating the hotel" });
            }
        }

        /// <summary>
        /// Update an existing hotel
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(Hotel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] Hotel hotel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedHotel = await _hotelService.UpdateHotelAsync(id, hotel);
                if (updatedHotel == null)
                    return NotFound(new { message = $"Hotel with ID {id} not found" });

                return Ok(updatedHotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotel: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while updating the hotel" });
            }
        }

        /// <summary>
        /// Delete a hotel
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _hotelService.DeleteHotelAsync(id);
                if (!result)
                    return NotFound(new { message = $"Hotel with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hotel: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the hotel" });
            }
        }
    }
}
