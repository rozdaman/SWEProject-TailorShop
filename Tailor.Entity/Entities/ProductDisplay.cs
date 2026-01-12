using Tailor.Entity.Entities.Enums;

namespace Tailor.Entity.Entities
{
    public class ProductDisplay
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public DisplayType DisplayType { get; set; } // Enum
        public string ImageUrl { get; set; }
        public string Title { get; set; }
    }
}
