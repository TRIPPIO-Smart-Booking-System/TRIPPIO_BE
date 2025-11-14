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
            Console.WriteLine("[ReviewController.CreateReview] === Request started ===");
            Console.WriteLine($"[ReviewController.CreateReview] Request.OrderId: {request?.OrderId}");
            Console.WriteLine($"[ReviewController.CreateReview] Request.Rating: {request?.Rating}");
            Console.WriteLine($"[ReviewController.CreateReview] ModelState valid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("[ReviewController.CreateReview] ModelState is invalid");
                return BadRequest(ModelState);
            }

            var customerId = GetCustomerIdFromToken();
            Console.WriteLine($"[ReviewController.CreateReview] CustomerId extracted: {(customerId == Guid.Empty ? "EMPTY ❌" : customerId)}");
            
            if (customerId == Guid.Empty)
            {
                Console.WriteLine("[ReviewController.CreateReview] CustomerId is empty - checking token claims");
                var claims = User.Claims;
                foreach (var claim in claims)
                {
                    Console.WriteLine($"  - Claim: {claim.Type} = {claim.Value}");
                }
                return Unauthorized("Customer ID not found in token.");
            }

            try
            {
                var review = await _reviewService.CreateReviewAsync(request, customerId);
                if (review == null)
                {
                    Console.WriteLine("[ReviewController.CreateReview] Review creation returned null");
                    return BadRequest("Cannot review this order. Order must have a completed payment and belong to you.");
                }

                Console.WriteLine($"[ReviewController.CreateReview] ✓ Review created successfully: {review.Id}");
                return Ok(new { message = "Review created successfully", data = review });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[ReviewController.CreateReview] InvalidOperationException: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReviewController.CreateReview] Exception: {ex}");
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
            Console.WriteLine("[GetCustomerIdFromToken] === Token extraction started ===");
            Console.WriteLine($"[GetCustomerIdFromToken] User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"[GetCustomerIdFromToken] User.Identity.AuthenticationType: {User.Identity?.AuthenticationType}");
            
            // Try multiple claim types: "id" (UserClaims.Id), "CustomerId", or NameIdentifier
            var customerIdClaim = User.FindFirst("id")?.Value
                ?? User.FindFirst("CustomerId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            Console.WriteLine($"[GetCustomerIdFromToken] CustomerIdClaim value: {(string.IsNullOrEmpty(customerIdClaim) ? "NULL/EMPTY ❌" : customerIdClaim)}");
            
            if (Guid.TryParse(customerIdClaim, out var customerId))
            {
                Console.WriteLine($"[GetCustomerIdFromToken] ✓ Parsed CustomerId: {customerId}");
                return customerId;
            }

            Console.WriteLine($"[GetCustomerIdFromToken] ❌ Failed to parse CustomerId from claim");
            Console.WriteLine($"[GetCustomerIdFromToken] All claims:");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"  - Type: {claim.Type}, Value: {claim.Value}");
            }

            return Guid.Empty;
        }
    }
}
