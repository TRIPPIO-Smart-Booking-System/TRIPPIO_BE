using Trippio.Core.Models.Review;

namespace Trippio.Core.Services
{
    public interface IReviewService
    {
        Task<ReviewDto?> CreateReviewAsync(CreateReviewRequest request, Guid userId);
        Task<ReviewDto?> UpdateReviewAsync(int reviewId, UpdateReviewDto request, Guid userId);
        Task<bool> DeleteReviewAsync(int reviewId, Guid userId);
        Task<ReviewDto?> GetReviewByIdAsync(int reviewId);
        Task<IEnumerable<ReviewDto>> GetAllReviewsAsync();
        Task<IEnumerable<ReviewDto>> GetReviewsByOrderIdAsync(int orderId);
        Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(Guid userId);
        Task<bool> CanUserReviewOrderAsync(int orderId, Guid userId);
    }
}
