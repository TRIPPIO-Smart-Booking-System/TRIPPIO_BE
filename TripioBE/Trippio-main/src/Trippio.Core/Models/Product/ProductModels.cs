using System.ComponentModel.DataAnnotations;

namespace Trippio.Core.Models.Product
{
    public class CreateCategoryRequest
    {
        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }
    }

    public class CreateProductRequest
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(300)]
        public required string Name { get; set; }

        [Required]
        public decimal Price { get; set; }
    }

    public class CreateFeedbackRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    public class CreateCommentRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(2000)]
        public required string Content { get; set; }
    }

    public class UpdateInventoryRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Warehouse { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime DateCreated { get; set; }
        public int? Stock { get; set; }
        public string? Warehouse { get; set; }
    }

    public class FeedbackDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime DateCreated { get; set; }
    }
}