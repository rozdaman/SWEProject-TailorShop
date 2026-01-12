using System;

namespace Tailor.Entity.Entities
{
    public class StockLog
    {
        public int Id { get; set; }
        public int ProductId { get; set; } // No navigation needed usually for logs to keep them light, but Context has it?
        // Context didn't show relationship fluent api, so maybe just ID.
        // But PM uses it.
        public int? VariantId { get; set; }
        public decimal QuantityChange { get; set; }
        public decimal NewStockLevel { get; set; }
        public string MovementType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
    }
}
