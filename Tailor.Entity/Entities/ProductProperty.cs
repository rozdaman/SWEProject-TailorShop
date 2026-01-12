namespace Tailor.Entity.Entities
{
    public class ProductProperty
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
