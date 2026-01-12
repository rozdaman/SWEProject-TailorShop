using Tailor.Entity.Entities.Enums;

namespace Tailor.DTO.DTOs.ProductDtos
{
    public class UpdateProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public ProductType ProductType { get; set; }
        public int CategoryId { get; set; }
    }
}
