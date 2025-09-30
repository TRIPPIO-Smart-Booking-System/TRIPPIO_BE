using Trippio.Core.Domain.Entities;
using Trippio.Core.SeedWorks;

namespace Trippio.Core.Repositories
{
    public interface ICategoryRepository : IRepository<Category, int>
    {
        Task<Category?> GetByNameAsync(string name);
        Task<IEnumerable<Category>> GetAllWithProductCountAsync();
        Task<IEnumerable<Category>> GetActiveAsync();
        Task<IEnumerable<Category>> GetByParentIdAsync(int parentId);
        Task<Category?> GetBySlugAsync(string slug);
        Task<bool> IsSlugUniqueAsync(string slug, int excludeId = 0);
        Task<IEnumerable<Category>> GetRootCategoriesAsync();
        Task<IEnumerable<Category>> GetCategoryHierarchyAsync(int categoryId);
    }
}