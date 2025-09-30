namespace Trippio.Core.Models.Content
{
    public class ProductDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }
        
        // Related data
        public string? CategoryName { get; set; }
        public int? InventoryQuantity { get; set; }
    }
}