using Microsoft.EntityFrameworkCore;
using Tailor.Business.Abstract;
using Tailor.DataAccess.Abstract;
using Tailor.DTO.DTOs.StockDtos;
using Tailor.Entity.Entities;

namespace Tailor.Business.Concrete
{
    public class StockManager : IStockService
    {
        private readonly IStockDal _stockDal;
        private readonly IProductDal _productDal; // Ürün adını almak için lazım
        private readonly IProductVariantDal _productVariantDal; // Varyant stoğu için
        private readonly IStockLogDal _stockLogDal; // Loglama için

        public StockManager(IStockDal stockDal, IProductDal productDal, IProductVariantDal productVariantDal, IStockLogDal stockLogDal)
        {
            _stockDal = stockDal;
            _productDal = productDal;
            _productVariantDal = productVariantDal;
            _stockLogDal = stockLogDal;
        }

        // 1. STOK EKLEME (Mal Kabul)
        public void AddStock(UpdateStockDto dto)
        {
            if (dto.Quantity <= 0) throw new Exception("Invalid stock quantity.");

            // A. VARYANTLI ÜRÜN STOK GİRİŞİ
            if (dto.VariantId.HasValue)
            {
                var variant = _productVariantDal.GetById(dto.VariantId.Value);
                if (variant == null) throw new Exception("Varyant bulunamadı.");

                variant.StockQuantity += dto.Quantity;
                _productVariantDal.Update(variant);

                // Loglama
                LogMovement(dto.ProductId, dto.VariantId, dto.Quantity, variant.StockQuantity, "Mal Kabul (Varyant)", "Admin");
            }
            // B. STANDART ÜRÜN STOK GİRİŞİ (Kumaş vb.)
            else
            {
                var stock = _stockDal.GetByProductId(dto.ProductId);

                if (stock != null)
                {
                    // Varsa üzerine ekle
                    stock.Quantity += dto.Quantity;
                    if (!string.IsNullOrEmpty(dto.Location)) stock.Location = dto.Location;
                    _stockDal.Update(stock);

                    LogMovement(dto.ProductId, null, dto.Quantity, stock.Quantity, "Mal Kabul", "Admin");
                }
                else
                {
                    // Yoksa yeni stok kaydı aç
                    var newStock = new Stock
                    {
                        ProductId = dto.ProductId,
                        Quantity = dto.Quantity,
                        Location = dto.Location ?? "Merkez Depo"
                    };
                    _stockDal.Add(newStock);

                    LogMovement(dto.ProductId, null, dto.Quantity, dto.Quantity, "Mal Kabul (Yeni)", "Admin");
                }
            }
        }
        
        // 2. STOK DÜŞME (Satış Anı)
        public void DecreaseStock(int productId, decimal quantity, int? variantId = null)
        {
            // EĞER VARYANT VARSA: Varyant tablosundan düş
            if (variantId.HasValue)
            {
                var variant = _productVariantDal.GetById(variantId.Value);
                if (variant == null) throw new Exception("Varyant bulunamadı.");

                if (variant.StockQuantity < quantity)
                    throw new Exception($"Yetersiz Stok (Varyant)! Mevcut: {variant.StockQuantity}, İstenen: {quantity}");

                variant.StockQuantity -= quantity;
                _productVariantDal.Update(variant);

                LogMovement(productId, variantId, -quantity, variant.StockQuantity, "Satış", "System");
            }
            // EĞER VARYANT YOKSA: Ana Depo stoğundan düş
            else
            {
                var stock = _stockDal.GetByProductId(productId);
                if (stock == null) throw new Exception("Stok bulunamadı.");

                if (stock.Quantity < quantity)
                    throw new Exception($"Yetersiz Stok! Mevcut: {stock.Quantity}, İstenen: {quantity}");

                stock.Quantity -= quantity;
                _stockDal.Update(stock);

                LogMovement(productId, null, -quantity, stock.Quantity, "Satış", "System");
            }
        }


        private void LogMovement(int productId, int? variantId, decimal change, decimal newLevel, string type, string source = "System")
        {
            try
            {
                var log = new StockLog
                {
                    ProductId = productId,
                    VariantId = variantId,
                    QuantityChange = change,
                    NewStockLevel = newLevel,
                    MovementType = type,
                    CreatedAt = DateTime.Now,
                    Description = $"Otomatik log: {type}",
                    Source = source
                };
                _stockLogDal.Add(log);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LOGLAMA HATASI: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"DETAY: {ex.InnerException.Message}");
            }
        }

        // 3. STOK KONTROL (Sepet İçin)
        public bool CheckStock(int productId, decimal requestedQuantity, int? variantId = null)
        {
            if (variantId.HasValue)
            {
                var variant = _productVariantDal.GetById(variantId.Value);
                if (variant == null) return false;
                return variant.StockQuantity >= requestedQuantity;
            }
            else
            {
                var stock = _stockDal.GetByProductId(productId);
                // Stok yoksa veya miktar 0 ise false dön
                if (stock == null) return false;

                return stock.Quantity >= requestedQuantity;
            }
        }

        // 4. LİSTELEME
        public List<ResultStockDto> GetStockList()
        {
            // Burada Include işlemi için GenericManager'ın GetListWithRelations metodunu kullanabilirsin
            // Ama manuel olarak şöyle de yapabiliriz (Service katmanında Context kullanılmaz, DAL'a özel metot yazılmalıydı)
            // Şimdilik sadece Stock tablosunu dönüyoruz.

            var stocks = _stockDal.GetAll(); // İdealde Include(x=>x.Product) olmalı
            return stocks.Select(x => new ResultStockDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = "Ürün ID: " + x.ProductId, // Performans için Product ismini ayrı çekmedik
                Quantity = x.Quantity,
                Location = x.Location,
                IsLowStock = x.Quantity < 10 // Örn: 10'un altı kritiktir
            }).ToList();
        }

        public ResultStockDto GetStockByProductId(int productId)
        {
            var stock = _stockDal.GetByProductId(productId);

            // EĞER KAYIT YOKSA:
            if (stock == null)
            {
                return new ResultStockDto
                {
                    ProductId = productId, // <-- Bunu ekledik ki hangi ID olduğunu bilelim
                    Quantity = 0,
                    ProductName = "Stok Kaydı Oluşturulmamış", // <-- Bilgi notu
                    StockStatus = "Stok Yok"
                };
            }

            // KAYIT VARSA:
            return new ResultStockDto
            {
                Id = stock.Id,
                ProductId = stock.ProductId,
                Quantity = stock.Quantity,
                Location = stock.Location,
                // ... diğer alanlar
            };
        }

        public List<ResultStockLogDto> GetStockHistoryByVariant(int variantId)
        {
            var logs = _stockLogDal.GetAll(x => x.VariantId == variantId)
                                   .OrderByDescending(x => x.CreatedAt)
                                   .ToList();

            return logs.Select(x => new ResultStockLogDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                VariantId = x.VariantId,
                QuantityChange = x.QuantityChange,
                NewStockLevel = x.NewStockLevel,
                MovementType = x.MovementType,
                CreatedAt = x.CreatedAt,
                Description = x.Description,
                Source = x.Source
            }).ToList();
        }
    }
}