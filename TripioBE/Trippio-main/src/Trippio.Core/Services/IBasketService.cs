using Trippio.Core.Models;
using Trippio.Core.Models.Common;
using Trippio.Core.Models.Basket;

namespace Trippio.Core.Services
{
    public interface IBasketService
    {
        Task<BaseResponse<BasketDto>> GetByUserIdAsync(Guid userId);
        Task<BaseResponse<bool>> ClearBasketAsync(Guid userId);
        Task<BaseResponse<int>> GetItemCountAsync(Guid userId);
        Task<BaseResponse<decimal>> GetTotalAsync(Guid userId);
        Task<BaseResponse<BasketDto>> TransferBasketAsync(Guid fromUserId, Guid toUserId);
    }
}