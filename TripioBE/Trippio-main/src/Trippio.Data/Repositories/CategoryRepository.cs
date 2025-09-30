using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Core.Models;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class CategoryRepository : RepositoryBase<Category, int>, ICategoryRepository
    {
        public CategoryRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetActiveAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetByParentIdAsync(int parentId)
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetBySlugAsync(string slug)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.Contains(slug));
        }

        public async Task<bool> IsSlugUniqueAsync(string slug, int excludeId = 0)
        {
            return !await _context.Categories
                .AnyAsync(c => c.Name.Contains(slug) && c.Id != excludeId);
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoryHierarchyAsync(int categoryId)
        {
            var categories = new List<Category>();
            var currentCategory = await _context.Categories.FindAsync(categoryId);
            
            if (currentCategory != null)
            {
                categories.Add(currentCategory);
            }
            
            return categories;
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<IEnumerable<Category>> GetAllWithProductCountAsync()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();
        }
    }
}
