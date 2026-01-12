using AutoMapper;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.Business.Abstract;
using Tailor.DataAccess.Abstract;
using Tailor.DTO.DTOs.ProductDtos;
using Tailor.DTO.DTOs.StockDtos;
using Tailor.Entity.Entities;
using Tailor.Entity.Entities.Enums;

namespace Tailor.Business.Concrete
{
    public class ProductManager : GenericManager<Product>, IProductService
    {
        private readonly IProductDal _productDal;
        private readonly IMapper _mapper;
        private readonly IProductDisplayDal _productDisplayDal;
        private readonly IProductVariantDal _productVariantDal; // Varyant için
        private readonly IStockLogDal _stockLogDal; // Loglama için

        // Constructor Injection
        public ProductManager(IProductDal productDal, IMapper mapper,IProductDisplayDal productDisplayDal, IProductVariantDal productVariantDal, IStockLogDal stockLogDal) : base(productDal)
        {
            _productDal = productDal;
            _mapper = mapper;
            _productDisplayDal = productDisplayDal;
            _productVariantDal = productVariantDal;
            _stockLogDal = stockLogDal;
        }

        // ... (Diğer metotlar aynı kalacak) ...
        

        // =========================================================================
        // 1. MEVCUT METOTLAR (CRUD ve Detay)
        // =========================================================================

        public void CreateProduct(CreateProductDto createProductDto)
        {
            var product = _mapper.Map<Product>(createProductDto);

            // Kumaş ve Kıyafet ayrımı (Kritik İş Mantığı)
            if (product.ProductType == ProductType.Fabric)
            {
                // Kumaş ise: Varyant olamaz, Ana Stok tablosuna yazılır.
                if (product.Stock == null) product.Stock = new Stock();
                // Not: Quantity bilgisi DTO'da mapping ile Stock içine taşınmış olmalı
                // veya burada manuel atanmalı: product.Stock.Quantity = ... 
                // --- HATA ÇÖZÜMÜ BURADA ---
                // Veritabanı Location alanını zorunlu tutuyor, boş geçemeyiz.
                product.Stock.Location = "Merkez Depo"; // Varsayılan bir yer atıyoruz.

                // Eğer DTO'dan gelen stok miktarı Mapping ile dolmadıysa, manuel atayalım:
                if (createProductDto.StockQuantity.HasValue)
                {
                    product.Stock.Quantity = createProductDto.StockQuantity.Value;
                }
                product.ProductVariants = null;
            }
            else if (product.ProductType == ProductType.ReadyToWear)
            {
                // Kıyafet ise: Ana Stok tablosu boştur.
                product.Stock = null;
                // Not: Varyant zorunluluğu esnetildi. Boş bırakılabilir.
            }

            // Oluşturulma tarihini otomatik atayalım
            product.CretaedAt = DateTime.Now;

            try
            {
                _productDal.Add(product);
            }
            catch (Exception ex)
            {
                // Hatanın kök nedenini yakalayıp ekrana basacağız
                string detay = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception($"VERİTABANI HATASI: {detay}");
            }
        }

        public List<ResultProductListDto> GetProductsWithCategory()
        {
            // Kategorisiyle beraber tüm ürünleri çek
            var values = _productDal.GetListWithRelations(null, "Category", "Stock", "ProductVariants");
            return _mapper.Map<List<ResultProductListDto>>(values);
        }

        public ResultProductDto GetProductDetailsById(int id)
        {
            // Detay sayfasında her şeye ihtiyacımız var (Include zinciri)
            var values = _productDal.GetListWithRelations(
                x => x.id == id,
                "Category",
                "Stock",
                "ProductVariants",
                "ProductProperties",
                "ProductTags.Tag" // Çoka-çok ilişki detayına inme
            ).FirstOrDefault();

            if (values == null) return null;

            return _mapper.Map<ResultProductDto>(values);
        }

        // =========================================================================
        // 2. ARAMA VE FİLTRELEME
        // =========================================================================

        public List<ResultProductListDto> SearchProducts(string keyword)
        {
            // İsimde VEYA Açıklamada aranan kelime geçiyorsa getir (Case insensitive genelde veritabanı ayarına bağlıdır)
            var values = _productDal.GetListByFilter(x =>
                x.Name.Contains(keyword) ||
                x.Description.Contains(keyword)
            );

            return _mapper.Map<List<ResultProductListDto>>(values);
        }

        public List<ResultProductListDto> GetProductsByPriceRange(decimal minPrice, decimal maxPrice)
        {
            // Belirli fiyat aralığındaki ürünler
            var values = _productDal.GetListByFilter(x => x.price >= minPrice && x.price <= maxPrice);
            return _mapper.Map<List<ResultProductListDto>>(values);
        }

        public List<ResultProductListDto> GetProductsByType(ProductType productType)
        {
            // Sadece Kumaşları veya Sadece Kıyafetleri getir
            var values = _productDal.GetListWithRelations(x => x.ProductType == productType, "Category", "Stock", "ProductVariants");
            return _mapper.Map<List<ResultProductListDto>>(values);
        }

        public List<ResultProductListDto> GetFilteredProducts(int? categoryId, string? size)
        {
            // 1. Filtreyi Oluştur
            Expression<Func<Product, bool>> filter = x => x.IsActive;

            if (categoryId.HasValue)
            {
                // Hiyerarşik: Hem seçilen kategori hem de alt kategorileri gelsin mi? 
                // Şimdilik sadece seçilen kategoriyi filtreliyoruz.
                var originalFilter = filter;
                filter = x => x.IsActive && x.CategoryId == categoryId.Value;
            }

            // 2. Veriyi Çek (Hızlı olması için AsNoTracking mantığı Generic DAL içinde veya burada kurgulanmalı)
            // Bizim GetListWithRelations metodumuz şu an List dönüyor (yani ToList yapılmış durumda).
            var products = _productDal.GetListWithRelations(filter, "Category", "ProductVariants");

            // 3. Bellekte Boyut (Size) Filtresi Uygula (Varyant tablosu join gibi çalışır)
            if (!string.IsNullOrEmpty(size))
            {
                products = products.Where(p => p.ProductVariants != null && 
                                              p.ProductVariants.Any(v => v.Size == size))
                                   .ToList();
            }

            return _mapper.Map<List<ResultProductListDto>>(products);
        }

        // =========================================================================
        // 3. VİTRİN VE ÖNE ÇIKANLAR
        // =========================================================================

       /* public List<ResultProductListDto> GetLastAddedProducts(int count)
        {
            // Generic DAL'da OrderBy metodu olmadığı için veriyi çekip bellekte sıralıyoruz.
            // (Performans notu: İlerde DAL'a GetLastItems metodu eklenebilir)
            var values = _productDal.GetListWithRelations(null, "Category")
                .OrderByDescending(x => x.CretaedAt)
                .Take(count)
                .ToList();

            return _mapper.Map<List<ResultProductListDto>>(values);
        }*/ 
       //metodu ürün kullanımının ilerleyen zamanlarında performans için şeklinde değiştirildi.
        public List<ResultProductListDto> GetLastAddedProducts(int count)
        {
            // ESKİ YÖNTEM (Kötü Performans):
            // var values = _productDal.GetListWithRelations(null, "Category")... .ToList();

            // YENİ YÖNTEM (Yüksek Performans):
            // SQL tarafında filtrelenmiş veriyi direkt çekiyoruz.
            var values = _productDal.GetLastProducts(count);

            return _mapper.Map<List<ResultProductListDto>>(values);
        }
        public List<ResultProductListDto> GetShowcaseProducts(DisplayType displayType)
        {
            // İlişkili tablodan (ProductDisplay) sorgulama yapıyoruz.
            // "Bana öyle ürünleri getir ki, ProductDisplays listesinde 'displayType' eşleşen bir kayıt olsun."
            var values = _productDal.GetListWithRelations(
                x => x.ProductDisplays.Any(pd => pd.DisplayType == displayType),
                "Category", "Stock", "ProductVariants"
            );

            return _mapper.Map<List<ResultProductListDto>>(values);
        }

        // =========================================================================
        // 4. PERFORMANS VE SAYFALAMA
        // =========================================================================

        public PagedResultDto<ResultProductListDto> GetProductsWithPaging(int page, int size)
        {
            // Toplam sayıyı al
            int totalCount = _productDal.GetCount();

            // Sayfalama (Memory Paging currently per DAL limitation)
            var values = _productDal.GetListWithRelations(null, "Category", "Stock", "ProductVariants")
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();

            var mappedItems = _mapper.Map<List<ResultProductListDto>>(values);

            return new PagedResultDto<ResultProductListDto>
            {
                Items = mappedItems,
                TotalCount = totalCount,
                Page = page,
                Size = size
            };
        }

        // =========================================================================
        // 5. ADMİN / YÖNETİM METOTLARI
        // =========================================================================

        public void UpdateStock(int productId, decimal newQuantity, int? variantId = null)
        {
             // Loglama
            decimal oldQuantity = 0;

            if (variantId.HasValue)
            {
                var variant = _productVariantDal.GetById(variantId.Value);
                if (variant != null)
                {
                    oldQuantity = variant.StockQuantity;
                    variant.StockQuantity = newQuantity;
                    _productVariantDal.Update(variant);
                }
                else throw new Exception("Varyant bulunamadı.");
            }
            else
            {
                var product = _productDal.GetListWithRelations(x => x.id == productId, "Stock").FirstOrDefault();
                if (product != null && product.Stock != null)
                {
                     oldQuantity = product.Stock.Quantity;
                     product.Stock.Quantity = newQuantity;
                     _productDal.Update(product); // Cascade update yapar
                }
            }

            // Değişikliği Logla
            var log = new StockLog
            {
                ProductId = productId,
                VariantId = variantId,
                QuantityChange = newQuantity - oldQuantity, // Fark
                NewStockLevel = newQuantity,
                MovementType = "Sayım/Düzeltme",
                CreatedAt = DateTime.Now,
                Description = "Admin panelinden direkt güncelleme"
            };
            _stockLogDal.Add(log);
        }

        public void ToggleProductStatus(int id)
        {
            var product = _productDal.GetById(id);
            if (product != null)
            {
                // Aktifse Pasif, Pasifse Aktif yap (Tersine çevir)
                product.IsActive = !product.IsActive;
                _productDal.Update(product);
            }
        }

        public void AddProductToShowcase(CreateProductDisplayDto dto)
        {
            // 1. Önce bu ürün zaten o vitrinde ekli mi? Kontrol edelim (Çift kayıt olmasın)
            // (Generic DAL'da GetList varsa filter ile bakabilirsin, yoksa direkt ekle)

            var display = _mapper.Map<ProductDisplay>(dto);

            // Eğer resim göndermediyse, ürünün kendi resmini kullansın diye bir mantık kurabilirsin
            if (string.IsNullOrEmpty(display.ImageUrl))
            {
                var product = _productDal.GetById(dto.ProductId);
                display.ImageUrl = product?.ImageUrl ?? "no-image.jpg";
            }

            _productDisplayDal.Add(display);
        }

        public List<ResultStockLogDto> GetStockHistory(int productId)
        {
            // O ürüne ait tüm logları getir, tarihe göre yeniden eskiye sırala
            var logs = _stockLogDal.GetAll(x => x.ProductId == productId)
                                   .OrderByDescending(x => x.CreatedAt)
                                   .ToList();

            return _mapper.Map<List<ResultStockLogDto>>(logs);
        }
    }
}