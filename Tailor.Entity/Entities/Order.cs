using System;
using System.Collections.Generic;

namespace Tailor.Entity.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public AppUser User { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public Tailor.Entity.Entities.Enums.OrderStatus Status { get; set; }

        public int ShippingAddressId { get; set; }
        public Address ShippingAddress { get; set; }

        public int BillingAddressId { get; set; }
        public Address BillingAddress { get; set; }
        public string OrderNumber { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public Shipping Shipping { get; set; }
    }
}
