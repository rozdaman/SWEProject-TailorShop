namespace Tailor.Entity.Entities
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public decimal StockQuantity { get; set; }
        public string ImageUrl { get; set; }
    }
}
