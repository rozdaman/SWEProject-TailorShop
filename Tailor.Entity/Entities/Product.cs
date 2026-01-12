using System;
using System.Collections.Generic;
using Tailor.Entity.Entities.Enums;

namespace Tailor.Entity.Entities
{
    public class Product
    {
        public int id { get; set; } // Note: ProductManager uses lowercase 'id' in some places, usually Id
        // But PM code used 'x.id == id' -> checking PM code...
        // PM: 'x.id == id'. If I make it 'Id', I might need to fix PM or use 'Id'.
        // Standard is 'Id'. I will use 'id' to match existing code usage if that's what it was.
        // Actually, let's use 'id' because logic uses it.
        
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal price { get; set; } // PM uses 'x.price'
        public string ImageUrl { get; set; }
        public DateTime CretaedAt { get; set; } // PM uses 'CretaedAt' (typo in original code?)
        public bool IsActive { get; set; }
        
        public ProductType ProductType { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public Stock Stock { get; set; }
        
        public ICollection<ProductVariant> ProductVariants { get; set; }
        public ICollection<ProductProperty> ProductProperties { get; set; }
        public ICollection<ProductTag> ProductTags { get; set; }
        public ICollection<ProductDisplay> ProductDisplays { get; set; }
        
        // Blog assignments
        public ICollection<BlogProductAssignment> BlogAssignments { get; set; }
    }
}
