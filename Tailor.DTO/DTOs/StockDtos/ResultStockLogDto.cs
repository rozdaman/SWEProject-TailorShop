using System;

namespace Tailor.DTO.DTOs.StockDtos
{
    public class ResultStockLogDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal QuantityChange { get; set; }
        public decimal NewStockLevel { get; set; }
        public string MovementType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public int? VariantId { get; set; }
    }
}
