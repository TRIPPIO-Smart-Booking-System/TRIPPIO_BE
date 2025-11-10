using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trippio.Core.Models.Review;
using Trippio.Core.Services;
using Trippio.Core.SeedWorks.Constants;

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

            var userId = GetUserIdFromToken();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var review = await _reviewService.CreateReviewAsync(request, userId);
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

            var userId = GetUserIdFromToken();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token.");
            }

            var review = await _reviewService.UpdateReviewAsync(reviewId, request, userId);
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
            var userId = GetUserIdFromToken();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token.");
            }

            var result = await _reviewService.DeleteReviewAsync(reviewId, userId);
            if (!result)
            {
                return NotFound("Review not found or you don't have permission to delete it.");
            }

            return Ok(new { message = "Review deleted successfully" });
        }

        /// <summary>
        /// Get all reviews (with customer and order information)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllReviews()
        {
            try
            {
                var reviews = await _reviewService.GetAllReviewsAsync();
                return Ok(new { data = reviews, count = reviews.Count() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving reviews", error = ex.Message });
            }
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
            var userId = GetUserIdFromToken();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token.");
            }

            var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
            return Ok(reviews);
        }

        /// <summary>
        /// Check if customer can review an order
        /// </summary>
        [HttpGet("can-review/{orderId}")]
        public async Task<IActionResult> CanReviewOrder(int orderId)
        {
            var userId = GetUserIdFromToken();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token.");
            }

            var canReview = await _reviewService.CanUserReviewOrderAsync(orderId, userId);
            return Ok(new { canReview = canReview });
        }

        private Guid GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(UserClaims.Id)?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return Guid.Empty;
        }
    }
}
