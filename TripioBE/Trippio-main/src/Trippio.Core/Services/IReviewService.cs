using Trippio.Core.Models.Review;

namespace Trippio.Core.Services
{
    public interface IReviewService
    {
        Task<ReviewDto?> CreateReviewAsync(CreateReviewRequest request, Guid customerId);
        Task<ReviewDto?> UpdateReviewAsync(int reviewId, UpdateReviewDto request, Guid customerId);
        Task<bool> DeleteReviewAsync(int reviewId, Guid customerId);
        Task<ReviewDto?> GetReviewByIdAsync(int reviewId);
        Task<IEnumerable<ReviewDto>> GetReviewsByOrderIdAsync(int orderId);
        Task<IEnumerable<ReviewDto>> GetReviewsByCustomerIdAsync(Guid customerId);
        Task<bool> CanCustomerReviewOrderAsync(int orderId, Guid customerId);
    }
}
