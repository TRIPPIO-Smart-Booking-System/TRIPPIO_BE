using AutoMapper;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Models;
using Trippio.Core.Models.Common;
using Trippio.Core.Models.Content;
using Trippio.Core.Repositories;
using Trippio.Core.Services;
using Trippio.Core.SeedWorks;

namespace Trippio.Data.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResponse<IEnumerable<ProductDto>>> GetAllActiveAsync()
        {
            try
            {
                var products = await _productRepository.GetActiveAsync();
                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
                return BaseResponse<IEnumerable<ProductDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<ProductDto>>.ServerError($"Error retrieving products: {ex.Message}");
            }
        }

        public async Task<BaseResponse<ProductDto>> GetByIdAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return BaseResponse<ProductDto>.NotFound("Product not found");

                var productDto = _mapper.Map<ProductDto>(product);
                return BaseResponse<ProductDto>.Success(productDto);
            }
            catch (Exception ex)
            {
                return BaseResponse<ProductDto>.ServerError($"Error retrieving product: {ex.Message}");
            }
        }

        public async Task<BaseResponse<ProductDto>> GetBySlugAsync(string slug)
        {
            try
            {
                var product = await _productRepository.GetBySlugAsync(slug);
                if (product == null)
                    return BaseResponse<ProductDto>.NotFound("Product not found");

                var productDto = _mapper.Map<ProductDto>(product);
                return BaseResponse<ProductDto>.Success(productDto);
            }
            catch (Exception ex)
            {
                return BaseResponse<ProductDto>.ServerError($"Error retrieving product: {ex.Message}");
            }
        }

        public async Task<BaseResponse<IEnumerable<ProductDto>>> GetByCategoryIdAsync(int categoryId)
        {
            try
            {
                var products = await _productRepository.GetByCategoryIdAsync(categoryId);
                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
                return BaseResponse<IEnumerable<ProductDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<ProductDto>>.ServerError($"Error retrieving products: {ex.Message}");
            }
        }

        public async Task<BaseResponse<PageResult<ProductDto>>> GetPagedAsync(ProductSearchDto searchDto)
        {
            try
            {
                var result = await _productRepository.GetPagedAsync(
                    searchDto.PageIndex, 
                    searchDto.PageSize, 
                    searchDto.CategoryId, 
                    searchDto.SearchTerm);

                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(result.Items);
                
                var pagedResult = new PageResult<ProductDto>
                {
                    Items = productDtos,
                    TotalItems = result.TotalItems,
                    PageIndex = result.PageIndex,
                    PageSize = result.PageSize
                };

                return BaseResponse<PageResult<ProductDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                return BaseResponse<PageResult<ProductDto>>.ServerError($"Error retrieving products: {ex.Message}");
            }
        }

        public async Task<BaseResponse<IEnumerable<ProductDto>>> GetFeaturedAsync()
        {
            try
            {
                var products = await _productRepository.GetFeaturedAsync();
                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
                return BaseResponse<IEnumerable<ProductDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<ProductDto>>.ServerError($"Error retrieving featured products: {ex.Message}");
            }
        }

        public async Task<BaseResponse<IEnumerable<ProductDto>>> SearchAsync(string searchTerm)
        {
            try
            {
                var products = await _productRepository.SearchAsync(searchTerm);
                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
                return BaseResponse<IEnumerable<ProductDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<ProductDto>>.ServerError($"Error searching products: {ex.Message}");
            }
        }

        public async Task<BaseResponse<ProductDto>> CreateAsync(CreateProductDto createDto)
        {
            try
            {
                // Validate category exists
                var category = await _categoryRepository.GetByIdAsync(createDto.CategoryId);
                if (category == null)
                    return BaseResponse<ProductDto>.Error("Category not found", "INVALID_CATEGORY");

                // Check slug uniqueness
                if (!await _productRepository.IsSlugUniqueAsync(createDto.Slug))
                    return BaseResponse<ProductDto>.Error("Slug already exists", "SLUG_EXISTS");

                var product = _mapper.Map<Product>(createDto);
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                await _productRepository.AddAsync(product);
                await _unitOfWork.CommitAsync();

                var productDto = _mapper.Map<ProductDto>(product);
                return BaseResponse<ProductDto>.Success(productDto);
            }
            catch (Exception ex)
            {
                return BaseResponse<ProductDto>.ServerError($"Error creating product: {ex.Message}");
            }
        }

        public async Task<BaseResponse<ProductDto>> UpdateAsync(int id, UpdateProductDto updateDto)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return BaseResponse<ProductDto>.NotFound("Product not found");

                // Validate category if changed
                if (updateDto.CategoryId != product.CategoryId)
                {
                    var category = await _categoryRepository.GetByIdAsync(updateDto.CategoryId);
                    if (category == null)
                        return BaseResponse<ProductDto>.Error("Category not found", "INVALID_CATEGORY");
                }

                // Check slug uniqueness if changed
                if (updateDto.Slug != product.Slug && !await _productRepository.IsSlugUniqueAsync(updateDto.Slug, id))
                    return BaseResponse<ProductDto>.Error("Slug already exists", "SLUG_EXISTS");

                _mapper.Map(updateDto, product);
                product.UpdatedAt = DateTime.UtcNow;

                await _productRepository.UpdateAsync(product);
                await _unitOfWork.CommitAsync();

                var productDto = _mapper.Map<ProductDto>(product);
                return BaseResponse<ProductDto>.Success(productDto);
            }
            catch (Exception ex)
            {
                return BaseResponse<ProductDto>.ServerError($"Error updating product: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return BaseResponse<bool>.NotFound("Product not found");

                await _productRepository.DeleteAsync(id);
                await _unitOfWork.CommitAsync();

                return BaseResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.ServerError($"Error deleting product: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> ToggleActiveStatusAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return BaseResponse<bool>.NotFound("Product not found");

                product.IsActive = !product.IsActive;
                product.UpdatedAt = DateTime.UtcNow;

                await _productRepository.UpdateAsync(product);
                await _unitOfWork.CommitAsync();

                return BaseResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.ServerError($"Error toggling product status: {ex.Message}");
            }
        }
    }
}