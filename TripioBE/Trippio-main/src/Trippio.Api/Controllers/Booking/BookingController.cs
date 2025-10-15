using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trippio.Core.Services;
using Trippio.Data;

namespace Trippio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _bookingService.GetByIdAsync(id);
            return StatusCode(result.Code, result);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            var result = await _bookingService.GetByUserIdAsync(userId);
            return StatusCode(result.Code, result);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var result = await _bookingService.GetByStatusAsync(status);
            return StatusCode(result.Code, result);
        }

        [HttpGet("upcoming/{userId:guid}")]
        public async Task<IActionResult> GetUpcoming(Guid userId)
        {
            var result = await _bookingService.GetUpcomingBookingsAsync(userId);
            return StatusCode(result.Code, result);
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] string status)
        {
            var result = await _bookingService.UpdateStatusAsync(id, status);
            return StatusCode(result.Code, result);
        }

        [HttpPut("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromQuery] Guid userId)
        {
            var result = await _bookingService.CancelBookingAsync(id, userId);
            return StatusCode(result.Code, result);
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotal([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var result = await _bookingService.GetTotalBookingValueAsync(from, to);
            return StatusCode(result.Code, result);
        }

        [HttpGet("debug/db")]
        public async Task<IActionResult> DebugDb([FromServices] TrippioDbContext db)
        {
            var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT @@SERVERNAME AS ServerName, DB_NAME() AS DbName";
            using var reader = await cmd.ExecuteReaderAsync();
            string server = "", dbname = "";
            if (await reader.ReadAsync())
            {
                server = reader.GetString(0);
                dbname = reader.GetString(1);
            }
            return Ok(new { server, dbname, conn = conn.ConnectionString });
        }


        [HttpGet("debug/columns")]
        public IActionResult DebugColumns([FromServices] TrippioDbContext db)
        {
            var columns = db.RawCols
                .FromSqlRaw("SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Bookings'")
                .ToList();

            return Ok(columns);
        }

   
    }
}

