namespace Tailor.Entity.Entities
{
    public class Shipping
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
        public string Status { get; set; }
    }
}
