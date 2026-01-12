using System.Collections.Generic;

namespace Tailor.Entity.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ProductTag> ProductTags { get; set; }
    }

    public class ProductTag
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
