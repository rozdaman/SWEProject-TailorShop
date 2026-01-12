using System;

namespace Tailor.Entity.Entities
{
    public class TailoredProduct
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        // ...
    }
    
    public class ContactMessage 
    { 
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Payment 
    { 
        public int Id { get; set; } 
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public decimal Amount { get; set; }
        public Tailor.Entity.Entities.Enums.PaymentMethod PaymentMethod { get; set; }
        // Wait, OrderManager Error 50: PaymentStatus enum implicitly to string error.
        // PaymentStatus in Entity is string in my previous view? 
        // Let's make PaymentStatus an Enum in Entity too.
        public Tailor.Entity.Entities.Enums.PaymentStatus Status { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TransactionId { get; set; }
    }

    public class Shipment
    {
        public int Id { get; set; } 
        public int OrderId { get; set; }
        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
        public Tailor.Entity.Entities.Enums.ShipmentStatus Status { get; set; }
        public DateTime ShippedDate { get; set; }
    }

    public class About { public int Id { get; set; } public string Title { get; set; } public string Description { get; set; } }
    public class Banner { public int Id { get; set; } public string Title { get; set; } public string ImageUrl { get; set; } }
    
    public class SearchLog 
    { 
        public int Id { get; set; } 
        public string Keyword { get; set; }
        public int? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class CustomerSocial { public int Id { get; set; } public string Platform { get; set; } public string Url { get; set; } }
}
