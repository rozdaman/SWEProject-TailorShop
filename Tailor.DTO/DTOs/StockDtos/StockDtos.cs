using System;

namespace Tailor.DTO.DTOs.StockDtos
{
    public class UpdateStockDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int? VariantId { get; set; }
        public string Location { get; set; }
    }

    public class ResultStockDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string Location { get; set; }
        public bool IsLowStock { get; set; }
        public string StockStatus { get; set; }
    }
}
