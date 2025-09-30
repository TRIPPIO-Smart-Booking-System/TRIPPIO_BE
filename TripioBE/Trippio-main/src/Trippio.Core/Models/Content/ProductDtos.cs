namespace Trippio.Core.Models.Content
{
    public class CreateProductDto
    {
        public int CategoryId { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class UpdateProductDto
    {
        public int CategoryId { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class ProductSearchDto
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}