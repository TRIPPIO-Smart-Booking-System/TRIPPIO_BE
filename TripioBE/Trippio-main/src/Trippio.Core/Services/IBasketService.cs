using Trippio.Core.Models.Basket;
using Trippio.Core.Models.Common;

namespace Trippio.Core.Services
{
    public interface IBasketService
    {
        Task<BaseResponse<Basket>> GetAsync(Guid userId, CancellationToken ct = default);

        Task<BaseResponse<Basket>> AddItemAsync(Guid userId, AddItemDto dto, CancellationToken ct = default);

        Task<BaseResponse<Basket>> UpdateQuantityAsync(Guid userId, UpdateItemQuantityDto dto, CancellationToken ct = default);

        Task<BaseResponse<Basket>> RemoveItemAsync(Guid userId, string productId, CancellationToken ct = default);

        Task<BaseResponse<bool>> ClearAsync(Guid userId, CancellationToken ct = default);
    }
}
