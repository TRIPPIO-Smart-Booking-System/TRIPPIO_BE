using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trippio.Core.Services;

namespace Trippio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _payments;

        public PaymentController(IPaymentService payments)
        {
            _payments = payments;
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            var result = await _payments.GetByUserIdAsync(userId);
            return StatusCode(result.Code, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _payments.GetByIdAsync(id);
            return StatusCode(result.Code, result);
        }

        [HttpGet("order/{orderId:int}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            var result = await _payments.GetByOrderIdAsync(orderId);
            return StatusCode(result.Code, result);
        }

        [HttpGet("booking/{bookingId:guid}")]
        public async Task<IActionResult> GetByBooking(Guid bookingId)
        {
            var result = await _payments.GetByBookingIdAsync(bookingId);
            return StatusCode(result.Code, result);
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] string status)
        {
            var result = await _payments.UpdatePaymentStatusAsync(id, status);
            return StatusCode(result.Code, result);
        }

        [HttpPut("{id:guid}/refund")]
        public async Task<IActionResult> Refund(Guid id, [FromQuery] decimal amount)
        {
            var result = await _payments.RefundPaymentAsync(id, amount);
            return StatusCode(result.Code, result);
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotal([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var result = await _payments.GetTotalPaymentAmountAsync(from, to);
            return StatusCode(result.Code, result);
        }
    }
}
