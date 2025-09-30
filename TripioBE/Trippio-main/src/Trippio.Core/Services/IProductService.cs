using Trippio.Core.Models;
using Trippio.Core.Models.Common;
using Trippio.Core.Models.Content;

namespace Trippio.Core.Services
{
    public interface IProductService
    {
        Task<BaseResponse<IEnumerable<ProductDto>>> GetAllActiveAsync();
        Task<BaseResponse<ProductDto>> GetByIdAsync(int id);
        Task<BaseResponse<ProductDto>> GetBySlugAsync(string slug);
        Task<BaseResponse<IEnumerable<ProductDto>>> GetByCategoryIdAsync(int categoryId);
        Task<BaseResponse<IEnumerable<ProductDto>>> GetFeaturedAsync();
        Task<BaseResponse<IEnumerable<ProductDto>>> SearchAsync(string searchTerm);
        Task<BaseResponse<bool>> DeleteAsync(int id);
        Task<BaseResponse<bool>> ToggleActiveStatusAsync(int id);
    }
}