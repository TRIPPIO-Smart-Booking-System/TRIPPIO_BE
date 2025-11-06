using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Models.Review;
using Trippio.Core.Repositories;
using Trippio.Core.Services;
using Trippio.Data;

namespace Trippio.Data.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly TrippioDbContext _context;
        private readonly IMapper _mapper;

        public ReviewService(
            IReviewRepository reviewRepository,
            TrippioDbContext context,
            IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ReviewDto?> CreateReviewAsync(CreateReviewRequest request, Guid customerId)
        {
            // Check if customer can review this order
            if (!await CanCustomerReviewOrderAsync(request.OrderId, customerId))
            {
                return null;
            }

            // Check if customer has already reviewed this order
            if (await _reviewRepository.HasCustomerReviewedOrderAsync(request.OrderId, customerId))
            {
                throw new InvalidOperationException("You have already reviewed this order.");
            }

            var review = new Review
            {
                OrderId = request.OrderId,
                CustomerId = customerId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.Add(review);
            await _context.SaveChangesAsync();

            // Load navigation properties
            var createdReview = await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.Id == review.Id);

            return _mapper.Map<ReviewDto>(createdReview);
        }

        public async Task<ReviewDto?> UpdateReviewAsync(int reviewId, UpdateReviewDto request, Guid customerId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null || review.CustomerId != customerId)
            {
                return null;
            }

            review.Rating = request.Rating;
            review.Comment = request.Comment;

            await _context.SaveChangesAsync();

            // Load navigation properties
            var updatedReview = await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            return _mapper.Map<ReviewDto>(updatedReview);
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, Guid customerId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null || review.CustomerId != customerId)
            {
                return false;
            }

            _reviewRepository.Remove(review);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int reviewId)
        {
            var review = await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            return review == null ? null : _mapper.Map<ReviewDto>(review);
        }

        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
        {
            var reviews = await _reviewRepository.GetAllReviewsAsync();
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByOrderIdAsync(int orderId)
        {
            var reviews = await _reviewRepository.GetReviewsByOrderIdAsync(orderId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByCustomerIdAsync(Guid customerId)
        {
            var reviews = await _reviewRepository.GetReviewsByCustomerIdAsync(customerId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<bool> CanCustomerReviewOrderAsync(int orderId, Guid customerId)
        {
            // Get the order with its payments
            var order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return false;
            }

            // Check if the order belongs to a user associated with this customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer == null)
            {
                return false;
            }

            // Check if the order belongs to the customer's user
            if (order.UserId != customer.UserId)
            {
                return false;
            }

            // Check if any payment for this order is completed (Paid status)
            var hasCompletedPayment = order.Payments.Any(p => p.Status == PaymentStatus.Paid);

            return hasCompletedPayment;
        }
    }
}
