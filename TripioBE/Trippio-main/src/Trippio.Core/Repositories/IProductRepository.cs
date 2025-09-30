using Trippio.Core.Domain.Entities;
using Trippio.Core.Models;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface IProductRepository : IRepository<Product, int>
    {
        Task<IEnumerable<Product>> GetActiveAsync();
        Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
        Task<PageResult<Product>> GetPagedAsync(int pageIndex, int pageSize, int? categoryId = null, string? searchTerm = null);
        Task<Product?> GetBySlugAsync(string slug);
        Task<bool> IsSlugUniqueAsync(string slug, int excludeId = 0);
        Task<IEnumerable<Product>> GetFeaturedAsync();
        Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<Product>> SearchAsync(string searchTerm);
        Task<IEnumerable<Product>> SearchByNameAsync(string searchTerm);
        Task<IEnumerable<Product>> GetTopRatedProductsAsync(int count);
        Task<Product?> GetWithInventoryAsync(int id);
    }

    public interface ICommentRepository : IRepository<Comment, int>
    {
        Task<IEnumerable<Comment>> GetByProductIdAsync(int productId);
        Task<PageResult<Comment>> GetPagedByProductIdAsync(int productId, int pageIndex, int pageSize);
    }

    public interface IProductInventoryRepository : IRepository<ProductInventory, int>
    {
        Task<ProductInventory?> GetByProductIdAsync(int productId);
        Task<bool> UpdateStockAsync(int productId, int newStock);
        Task<IEnumerable<ProductInventory>> GetLowStockProductsAsync(int threshold = 10);
    }
}