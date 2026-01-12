using Tailor.DTO.DTOs.StockDtos;

namespace Tailor.Business.Abstract
{
    public interface IStockService
    {
        // Depoya mal geldiğinde
        void AddStock(UpdateStockDto dto);

        // Sipariş verildiğinde (OrderManager buradan çağıracak)
        void DecreaseStock(int productId, decimal quantity, int? variantId = null);

        // Sepete eklerken kontrol için
        bool CheckStock(int productId, decimal requestedQuantity, int? variantId = null);

        // Admin paneli için listeleme
        List<ResultStockDto> GetStockList();

        // Tek bir ürünün stoğu
        ResultStockDto GetStockByProductId(int productId);

        // Stok Geçmişi (Senaryo 6)
        List<ResultStockLogDto> GetStockHistoryByVariant(int variantId);
    }
}