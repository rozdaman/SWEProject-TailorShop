using System;

namespace Tailor.DTO.DTOs.ProductPropertyDtos
{
    public class CreateProductPropertyDto
    {
        public int ProductId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class ResultProductPropertyDto : CreateProductPropertyDto { public int Id { get; set; } }
    
    public class CreateProductVariantDto
    {
         public int ProductId { get; set; }
         public string Size { get; set; }
         public string Color { get; set; }
         public decimal AdditionalPrice { get; set; }
         public decimal StockQuantity { get; set; }
         public string ImageUrl { get; set; }
    }
    public class ResultProductVariantDto : CreateProductVariantDto 
    { 
        public int Id { get; set; } 
        public decimal StockQuantity { get; set; }
    }
}
