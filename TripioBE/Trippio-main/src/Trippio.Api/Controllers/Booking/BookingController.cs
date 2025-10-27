using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trippio.Core.Models.Booking;
using Trippio.Core.Services;

namespace Trippio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService bookingService) => _bookingService = bookingService;

   
        [HttpPost("room")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomBookingRequest request, CancellationToken ct)
        {
            var result = await _bookingService.CreateRoomAsync(request);
            return StatusCode(result.Code, result);
        }

        [HttpPost("transport")]
        public async Task<IActionResult> CreateTransport([FromBody] CreateTransportBookingRequest request, CancellationToken ct)
        {
            var result = await _bookingService.CreateTransportAsync(request);
            return StatusCode(result.Code, result);
        }

        [HttpPost("show")]
        public async Task<IActionResult> CreateShow([FromBody] CreateShowBookingRequest request, CancellationToken ct)
        {
            var result = await _bookingService.CreateShowAsync(request);
            return StatusCode(result.Code, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _bookingService.GetByIdAsync(id);
            return StatusCode(result.Code, result);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUser(Guid userId, CancellationToken ct)
        {
            var result = await _bookingService.GetByUserIdAsync(userId);
            return StatusCode(result.Code, result);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status, CancellationToken ct)
        {
            var result = await _bookingService.GetByStatusAsync(status);
            return StatusCode(result.Code, result);
        }

        [HttpGet("upcoming/{userId:guid}")]
        public async Task<IActionResult> GetUpcoming(Guid userId, CancellationToken ct)
        {
            var result = await _bookingService.GetUpcomingBookingsAsync(userId);
            return StatusCode(result.Code, result);
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] string status, CancellationToken ct)
        {
            var result = await _bookingService.UpdateStatusAsync(id, status);
            return StatusCode(result.Code, result);
        }

        [HttpPut("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromQuery] Guid userId, CancellationToken ct)
        {
            var result = await _bookingService.CancelBookingAsync(id, userId);
            return StatusCode(result.Code, result);
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotal([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
        {
            var result = await _bookingService.GetTotalBookingValueAsync(from, to);
            return StatusCode(result.Code, result);
        }
    }
}
