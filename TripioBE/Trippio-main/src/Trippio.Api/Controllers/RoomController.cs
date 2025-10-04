using Microsoft.AspNetCore.Mvc;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Services;

namespace Trippio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<RoomController> _logger;

        public RoomController(IRoomService roomService, ILogger<RoomController> logger)
        {
            _roomService = roomService;
            _logger = logger;
        }

        /// <summary>
        /// Get all rooms
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Room>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var rooms = await _roomService.GetAllRoomsAsync();
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all rooms");
                return StatusCode(500, new { message = "An error occurred while retrieving rooms" });
            }
        }

        /// <summary>
        /// Get room by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Room), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var room = await _roomService.GetRoomByIdAsync(id);
                if (room == null)
                    return NotFound(new { message = $"Room with ID {id} not found" });

                return Ok(room);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room by ID: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the room" });
            }
        }

        /// <summary>
        /// Get room with hotel information
        /// </summary>
        [HttpGet("{id:guid}/hotel")]
        [ProducesResponseType(typeof(Room), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWithHotel(Guid id)
        {
            try
            {
                var room = await _roomService.GetRoomWithHotelAsync(id);
                if (room == null)
                    return NotFound(new { message = $"Room with ID {id} not found" });

                return Ok(room);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room with hotel: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the room" });
            }
        }

        /// <summary>
        /// Get rooms by hotel ID
        /// </summary>
        [HttpGet("hotel/{hotelId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<Room>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByHotelId(Guid hotelId)
        {
            try
            {
                var rooms = await _roomService.GetRoomsByHotelIdAsync(hotelId);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rooms by hotel ID: {HotelId}", hotelId);
                return StatusCode(500, new { message = "An error occurred while retrieving rooms" });
            }
        }

        /// <summary>
        /// Get available rooms by hotel ID
        /// </summary>
        [HttpGet("hotel/{hotelId:guid}/available")]
        [ProducesResponseType(typeof(IEnumerable<Room>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableRooms(Guid hotelId)
        {
            try
            {
                var rooms = await _roomService.GetAvailableRoomsAsync(hotelId);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available rooms: {HotelId}", hotelId);
                return StatusCode(500, new { message = "An error occurred while retrieving available rooms" });
            }
        }

        /// <summary>
        /// Create a new room
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Room), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Room room)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdRoom = await _roomService.CreateRoomAsync(room);
                return CreatedAtAction(nameof(GetById), new { id = createdRoom.Id }, createdRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");
                return StatusCode(500, new { message = "An error occurred while creating the room" });
            }
        }

        /// <summary>
        /// Update an existing room
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(Room), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] Room room)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedRoom = await _roomService.UpdateRoomAsync(id, room);
                if (updatedRoom == null)
                    return NotFound(new { message = $"Room with ID {id} not found" });

                return Ok(updatedRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while updating the room" });
            }
        }

        /// <summary>
        /// Delete a room
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _roomService.DeleteRoomAsync(id);
                if (!result)
                    return NotFound(new { message = $"Room with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the room" });
            }
        }
    }
}
