using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trippio.Core.Models.Review;
using Trippio.Core.Services;

namespace Trippio.Api.Controllers
{
    [Route("api/review")]
    [ApiController]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Create a new review for an order (requires completed payment)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customerId = GetCustomerIdFromToken();
            if (customerId == Guid.Empty)
            {
                return Unauthorized("Customer ID not found in token.");
            }

            try
            {
                var review = await _reviewService.CreateReviewAsync(request, customerId);
                if (review == null)
                {
                    return BadRequest("Cannot review this order. Order must have a completed payment and belong to you.");
                }

                return Ok(new { message = "Review created successfully", data = review });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the review", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing review
        /// </summary>
        [HttpPut("{reviewId}")]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customerId = GetCustomerIdFromToken();
            if (customerId == Guid.Empty)
            {
                return Unauthorized("Customer ID not found in token.");
            }

            var review = await _reviewService.UpdateReviewAsync(reviewId, request, customerId);
            if (review == null)
            {
                return NotFound("Review not found or you don't have permission to update it.");
            }

            return Ok(new { message = "Review updated successfully", data = review });
        }

        /// <summary>
        /// Delete a review
        /// </summary>
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var customerId = GetCustomerIdFromToken();
            if (customerId == Guid.Empty)
            {
                return Unauthorized("Customer ID not found in token.");
            }

            var result = await _reviewService.DeleteReviewAsync(reviewId, customerId);
            if (!result)
            {
                return NotFound("Review not found or you don't have permission to delete it.");
            }

            return Ok(new { message = "Review deleted successfully" });
        }

        /// <summary>
        /// Get a review by ID
        /// </summary>
        [HttpGet("{reviewId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewById(int reviewId)
        {
            var review = await _reviewService.GetReviewByIdAsync(reviewId);
            if (review == null)
            {
                return NotFound("Review not found.");
            }

            return Ok(review);
        }

        /// <summary>
        /// Get all reviews for a specific order
        /// </summary>
        [HttpGet("order/{orderId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewsByOrderId(int orderId)
        {
            var reviews = await _reviewService.GetReviewsByOrderIdAsync(orderId);
            return Ok(reviews);
        }

        /// <summary>
        /// Get all reviews by the current customer
        /// </summary>
        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews()
        {
            var customerId = GetCustomerIdFromToken();
            if (customerId == Guid.Empty)
            {
                return Unauthorized("Customer ID not found in token.");
            }

            var reviews = await _reviewService.GetReviewsByCustomerIdAsync(customerId);
            return Ok(reviews);
        }

        /// <summary>
        /// Check if customer can review an order
        /// </summary>
        [HttpGet("can-review/{orderId}")]
        public async Task<IActionResult> CanReviewOrder(int orderId)
        {
            var customerId = GetCustomerIdFromToken();
            if (customerId == Guid.Empty)
            {
                return Unauthorized("Customer ID not found in token.");
            }

            var canReview = await _reviewService.CanCustomerReviewOrderAsync(orderId, customerId);
            return Ok(new { canReview = canReview });
        }

        private Guid GetCustomerIdFromToken()
        {
            var customerIdClaim = User.FindFirst("CustomerId")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (Guid.TryParse(customerIdClaim, out var customerId))
            {
                return customerId;
            }

            return Guid.Empty;
        }
    }
}
