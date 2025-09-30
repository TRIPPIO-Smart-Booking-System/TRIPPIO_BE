using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trippio.Core.Models;
using Trippio.Core.Models.Basket;
using Trippio.Core.Services;

namespace Trippio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BasketController : ControllerBase
    {
        private readonly IBasketService _basketService;

        public BasketController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        /// <summary>
        /// Get current user's basket
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<BaseResponse<BasketDto>>> GetBasket()
        {
            var userId = GetCurrentUserId();
            var result = await _basketService.GetByUserIdAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Add item to basket
        /// </summary>
        [HttpPost("items")]
        public async Task<ActionResult<BaseResponse<BasketDto>>> AddItem([FromBody] AddBasketItemDto itemDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(BaseResponse<BasketDto>.Error(string.Join(", ", errors), "VALIDATION_ERROR"));
            }

            var userId = GetCurrentUserId();
            var result = await _basketService.AddItemAsync(userId, itemDto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Update item quantity in basket
        /// </summary>
        [HttpPut("items")]
        public async Task<ActionResult<BaseResponse<BasketDto>>> UpdateItemQuantity([FromBody] UpdateBasketItemDto itemDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(BaseResponse<BasketDto>.Error(string.Join(", ", errors), "VALIDATION_ERROR"));
            }

            var userId = GetCurrentUserId();
            var result = await _basketService.UpdateItemQuantityAsync(userId, itemDto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Remove item from basket
        /// </summary>
        [HttpDelete("items")]
        public async Task<ActionResult<BaseResponse<BasketDto>>> RemoveItem([FromBody] RemoveBasketItemDto itemDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(BaseResponse<BasketDto>.Error(string.Join(", ", errors), "VALIDATION_ERROR"));
            }

            var userId = GetCurrentUserId();
            var result = await _basketService.RemoveItemAsync(userId, itemDto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Clear entire basket
        /// </summary>
        [HttpDelete]
        public async Task<ActionResult<BaseResponse<bool>>> ClearBasket()
        {
            var userId = GetCurrentUserId();
            var result = await _basketService.ClearBasketAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get basket item count
        /// </summary>
        [HttpGet("count")]
        public async Task<ActionResult<BaseResponse<int>>> GetItemCount()
        {
            var userId = GetCurrentUserId();
            var result = await _basketService.GetItemCountAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get basket total amount
        /// </summary>
        [HttpGet("total")]
        public async Task<ActionResult<BaseResponse<decimal>>> GetTotal()
        {
            var userId = GetCurrentUserId();
            var result = await _basketService.GetTotalAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID");
            }
            return userId;
        }
    }
}