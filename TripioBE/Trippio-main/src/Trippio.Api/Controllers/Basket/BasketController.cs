using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trippio.Core.Models.Basket;
using Trippio.Core.Services;

namespace Trippio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class BasketController : ControllerBase
    {
        private readonly IBasketService _basket;

        public BasketController(IBasketService basket) => _basket = basket;

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> Get(Guid userId, CancellationToken ct)
        {
            var result = await _basket.GetAsync(userId, ct);
            return StatusCode(result.Code, result);
        }

        [HttpPost("{userId:guid}/items")]
        public async Task<IActionResult> Add(Guid userId, [FromBody] AddItemDto dto, CancellationToken ct)
        {
            var result = await _basket.AddItemAsync(userId, dto, ct);
            return StatusCode(result.Code, result);
        }

        [HttpPut("{userId:guid}/items/quantity")]
        public async Task<IActionResult> UpdateQuantity(Guid userId, [FromBody] UpdateItemQuantityDto dto, CancellationToken ct)
        {
            var result = await _basket.UpdateQuantityAsync(userId, dto, ct);
            return StatusCode(result.Code, result);
        }


        [HttpDelete("{userId:guid}/items/{bookingId:guid}")]
        public async Task<IActionResult> RemoveByPath(Guid userId, Guid bookingId, CancellationToken ct)
        {
            var result = await _basket.RemoveItemAsync(userId, bookingId, ct);
            return StatusCode(result.Code, result);
        }
        [HttpDelete("{userId:guid}/items")]
        public async Task<IActionResult> RemoveByQuery(Guid userId, [FromQuery] Guid bookingId, CancellationToken ct)
        {
            var result = await _basket.RemoveItemAsync(userId, bookingId, ct);
            return StatusCode(result.Code, result);
        }
        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> Clear(Guid userId, CancellationToken ct)
        {
            var result = await _basket.ClearAsync(userId, ct);
            return StatusCode(result.Code, result);
        }
    }
}
