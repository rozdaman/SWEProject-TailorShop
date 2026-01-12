using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.DTO.DTOs.ProductDtos;
using Tailor.DTO.DTOs.StockDtos;
using Tailor.Entity.Entities;
using Tailor.Entity.Entities.Enums;

namespace Tailor.Business.Abstract
{
    public interface IProductService : IGenericService<Product>
    {
        // --- 1. MEVCUT METOTLAR ---
        void CreateProduct(CreateProductDto createProductDto);
        List<ResultProductListDto> GetProductsWithCategory();
        ResultProductDto GetProductDetailsById(int id);

        // --- 2. ARAMA VE FİLTRELEME (Müşteri İçin Çok Önemli) ---

        // Arama çubuğu için: "İpek", "Kırmızı" yazınca gelen sonuçlar
        List<ResultProductListDto> SearchProducts(string keyword);

        // Fiyat aralığına göre filtreleme (Örn: 100 TL - 500 TL arası)
        List<ResultProductListDto> GetProductsByPriceRange(decimal minPrice, decimal maxPrice);

        // Sadece belirli bir türü getirme (Sadece Kumaşlar veya Sadece Kıyafetler)
        // Menüde "Kumaşlar"a tıklayınca çalışır.
        List<ResultProductListDto> GetProductsByType(ProductType productType);

        // Hiyerarşik ve Varyant Filtreleme (Senaryo 5)
        List<ResultProductListDto> GetFilteredProducts(int? categoryId, string? size);

        // --- 3. VİTRİN VE ÖNE ÇIKANLAR (Anasayfa İçin) ---

        // Anasayfa'da "Son Eklenen 10 Ürün"
        List<ResultProductListDto> GetLastAddedProducts(int count);

        // "Çok Satanlar" veya "Fırsat Ürünleri" vitrini için (ProductDisplay tablosundan)
        List<ResultProductListDto> GetShowcaseProducts(DisplayType displayType);

        // --- 4. PERFORMANS VE SAYFALAMA (Pagination) ---

        // 1000 ürün varsa hepsini birden çekmemek için (Sayfa 1, Sayfa 2...)
        // page: Hangi sayfa (1), size: Sayfada kaç ürün var (12)
        PagedResultDto<ResultProductListDto> GetProductsWithPaging(int page, int size);

        // --- 5. ADMİN / YÖNETİM METOTLARI ---

        // Hızlı Stok Güncelleme (Ürün detayına girmeden listeden stok değiştirmek için)
        void UpdateStock(int productId, decimal newQuantity, int? variantId = null);

        // Ürünü Pasife Çekme / Aktif Yapma (Silmek yerine gizlemek için)
        void ToggleProductStatus(int id);
        void AddProductToShowcase(CreateProductDisplayDto dto);
        List<ResultStockLogDto> GetStockHistory(int productId);
    }
}
