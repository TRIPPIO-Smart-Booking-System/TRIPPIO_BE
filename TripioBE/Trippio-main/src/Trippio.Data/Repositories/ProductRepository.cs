using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Repositories;
using Trippio.Core.Models;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class ProductRepository : RepositoryBase<Product, int>, IProductRepository
    {
        public ProductRepository(TrippioDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetActiveAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductInventory)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductInventory)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<PageResult<Product>> GetPagedAsync(int pageIndex, int pageSize, int? categoryId = null, string? searchTerm = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductInventory)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm));
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<Product>
            {
                Results = items,
                CurrentPage = pageIndex,
                PageSize = pageSize,
                RowCount = totalItems
            };
        }

        public async Task<Product?> GetWithInventoryAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductInventory)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> IsSlugExistsAsync(string slug, int excludeId = 0)
        {
            // Since slug doesn't exist in Product entity, return false
            return await Task.FromResult(false);
        }

        public async Task<Product?> GetBySlugAsync(string slug)
        {
            // Since slug doesn't exist in Product entity, return null
            return await Task.FromResult<Product?>(null);
        }

        public async Task<bool> IsSlugUniqueAsync(string slug, int excludeId = 0)
        {
            // Since slug doesn't exist in Product entity, return true
            return await Task.FromResult(true);
        }

        public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
        {
            return await SearchByNameAsync(searchTerm);
        }

        public async Task<IEnumerable<Product>> GetFeaturedAsync()
        {
            // Since IsFeatured doesn't exist, return top 10 products
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductInventory)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductInventory)
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchByNameAsync(string searchTerm)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetTopRatedProductsAsync(int count)
        {
            // Since rating doesn't exist, return top products by date
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductInventory)
                .OrderByDescending(p => p.DateCreated)
                .Take(count)
                .ToListAsync();
        }
    }
}
