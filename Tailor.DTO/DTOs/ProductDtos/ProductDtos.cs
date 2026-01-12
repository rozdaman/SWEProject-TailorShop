using System;
using System.Collections.Generic;
using Tailor.Entity.Entities.Enums;
using Tailor.DTO.DTOs.ProductPropertyDtos;

namespace Tailor.DTO.DTOs.ProductDtos
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public ProductType ProductType { get; set; }
        public int CategoryId { get; set; }
        public List<CreateProductVariantDto>? ProductVariants { get; set; }
        public decimal? StockQuantity { get; set; }
    }

    public class ResultProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; } // Note: Entity has 'price' (lowercase), make sure AutoMapper maps it
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; } // Added for filtering
        public string CategoryName { get; set; }
        public ProductType ProductType { get; set; }
        public DateTime CretaedAt { get; set; }
        public bool IsInStock { get; set; }
    }

    public class ResultProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public ProductType ProductType { get; set; }
        // Include collections if needed for details
        public List<ResultProductPropertyDto> ProductProperties { get; set; }
        public List<ResultProductVariantDto> ProductVariants { get; set; }
        public Tailor.DTO.DTOs.StockDtos.ResultStockDto Stock { get; set; }
    }



    public class CreateProductDisplayDto
    {
        public int ProductId { get; set; }
        public DisplayType DisplayType { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
    }
}
