using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trippio.Core.Models.Order;
using Trippio.Core.Services;

namespace Trippio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orders;

        public OrderController(IOrderService orders)
        {
            _orders = orders;
        }

        /// <summary>
        /// Get all orders (Admin/User feature)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var result = await _orders.GetAllAsync();
            return StatusCode(result.Code, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var result = await _orders.CreateOrderAsync(request);
            return StatusCode(result.Code, result);
        }

        [HttpPost("from-basket/{userId:guid}")]
        public async Task<IActionResult> CreateFromBasket(Guid userId, CancellationToken ct)
        {
            var result = await _orders.CreateFromBasketAsync(userId, ct);
            return StatusCode(result.Code, result);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            var result = await _orders.GetByUserIdAsync(userId);
            return StatusCode(result.Code, result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _orders.GetByIdAsync(id);
            return StatusCode(result.Code, result);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var result = await _orders.GetByStatusAsync(status);
            return StatusCode(result.Code, result);
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
        {
            var result = await _orders.UpdateStatusAsync(id, status);
            return StatusCode(result.Code, result);
        }

        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromQuery] Guid userId)
        {
            var result = await _orders.CancelOrderAsync(id, userId);
            return StatusCode(result.Code, result);
        }
        [HttpGet("revenue")]
        public async Task<IActionResult> Revenue([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var result = await _orders.GetTotalRevenueAsync(from, to);
            return StatusCode(result.Code, result);
        }

        [HttpGet("pending")]
        public async Task<IActionResult> Pending()
        {
            var result = await _orders.GetPendingOrdersAsync();
            return StatusCode(result.Code, result);
        }
    }
}
