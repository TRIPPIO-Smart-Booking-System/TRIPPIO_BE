using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trippio.Core.Models;
using Trippio.Core.Models.Product;
using Trippio.Core.Services;

namespace Trippio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Get all active products
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<BaseResponse<IEnumerable<ProductDto>>>> GetAllActive()
        {
            var result = await _productService.GetAllActiveAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BaseResponse<ProductDto>>> GetById(int id)
        {
            var result = await _productService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get product by slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<BaseResponse<ProductDto>>> GetBySlug(string slug)
        {
            var result = await _productService.GetBySlugAsync(slug);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("category/{categoryId:int}")]
        public async Task<ActionResult<BaseResponse<IEnumerable<ProductDto>>>> GetByCategory(int categoryId)
        {
            var result = await _productService.GetByCategoryIdAsync(categoryId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get paged products with search
        /// </summary>
        [HttpPost("search")]
        public async Task<ActionResult<BaseResponse<PageResult<ProductDto>>>> GetPaged([FromBody] ProductSearchDto searchDto)
        {
            var result = await _productService.GetPagedAsync(searchDto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get featured products
        /// </summary>
        [HttpGet("featured")]
        public async Task<ActionResult<BaseResponse<IEnumerable<ProductDto>>>> GetFeatured()
        {
            var result = await _productService.GetFeaturedAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Search products
        /// </summary>
        [HttpGet("search/{searchTerm}")]
        public async Task<ActionResult<BaseResponse<IEnumerable<ProductDto>>>> Search(string searchTerm)
        {
            var result = await _productService.SearchAsync(searchTerm);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Create new product (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse<ProductDto>>> Create([FromBody] CreateProductDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(BaseResponse<ProductDto>.Error(string.Join(", ", errors), "VALIDATION_ERROR"));
            }

            var result = await _productService.CreateAsync(createDto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Update product (Admin only)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse<ProductDto>>> Update(int id, [FromBody] UpdateProductDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(BaseResponse<ProductDto>.Error(string.Join(", ", errors), "VALIDATION_ERROR"));
            }

            var result = await _productService.UpdateAsync(id, updateDto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Delete product (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse<bool>>> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Toggle product active status (Admin only)
        /// </summary>
        [HttpPatch("{id:int}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse<bool>>> ToggleActiveStatus(int id)
        {
            var result = await _productService.ToggleActiveStatusAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}