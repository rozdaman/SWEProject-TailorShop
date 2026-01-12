using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tailor.Business.Abstract;
using Tailor.DTO.DTOs.ProductDtos;
using Tailor.Entity.Entities;
using Tailor.Entity.Entities.Enums;

namespace Tailor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        // Constructor Injection
        public ProductsController(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        // =========================================================================
        // 1. TEMEL CRUD İŞLEMLERİ
        // =========================================================================

        // Listeleme (İlişkili verilerle)
        // GET: api/products/getall
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var values = _productService.GetProductsWithCategory();
            return Ok(values);
        }

        // ID'ye Göre Detay Getirme
        // GET: api/products/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var value = _productService.GetProductDetailsById(id);
            if (value == null)
            {
                return NotFound("Ürün bulunamadı.");
            }
            return Ok(value);
        }

        // Yeni Ürün Ekleme (Kumaş/Kıyafet mantığı Business'ta çalışır)
        // POST: api/products
        [HttpPost]
        public IActionResult CreateProduct(CreateProductDto createProductDto)
        {
            try
            {
                _productService.CreateProduct(createProductDto);
                return Ok("Ürün başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                // Business katmanından fırlatılan hatayı (örn: varyant eksik) yakala
                return BadRequest(ex.Message);
            }
        }

        // Ürün Silme
        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var value = _productService.TGetById(id);
            if (value == null)
            {
                return NotFound("Silinecek ürün bulunamadı.");
            }
            _productService.TDelete(value);
            return Ok("Ürün başarıyla silindi.");
        }

        [HttpPut]
        public IActionResult UpdateProduct(UpdateProductDto updateProductDto)
        {
            // 1. Önce veritabanındaki gerçek kaydı bul
            var existingProduct = _productService.TGetById(updateProductDto.Id);

            if (existingProduct == null) return NotFound("Ürün bulunamadı");

            // 2. Sadece değişen alanları üzerine yaz (Map)
            // Bu sayede ID ve CreatedDate gibi alanlar korunur.
            _mapper.Map(updateProductDto, existingProduct);

            // 3. Güncelle
            _productService.TUpdate(existingProduct);

            return Ok("Ürün güncellendi.");
        }

        // =========================================================================
        // 2. ARAMA VE FİLTRELEME ENDPOINTLERİ
        // =========================================================================

        // Arama
        // GET: api/products/search?keyword=ipek
        [HttpGet("search")]
        public IActionResult SearchProducts([FromQuery] string keyword)
        {
            var values = _productService.SearchProducts(keyword);
            return Ok(values);
        }

        // Fiyat Aralığı
        // GET: api/products/filter-price?min=100&max=500
        [HttpGet("filter-price")]
        public IActionResult GetByPriceRange([FromQuery] decimal min, [FromQuery] decimal max)
        {
            var values = _productService.GetProductsByPriceRange(min, max);
            return Ok(values);
        }

        // Türüne Göre Getir (Kumaş veya Kıyafet)
        // GET: api/products/filter-type?type=0 (Fabric) veya type=1 (ReadyToWear)
        [HttpGet("filter-type")]
        public IActionResult GetByType([FromQuery] ProductType type)
        {
            var values = _productService.GetProductsByType(type);
            return Ok(values);
        }

        // =========================================================================
        // 3. VİTRİN VE SAYFALAMA ENDPOINTLERİ
        // =========================================================================

        // Son Eklenenler
        // GET: api/products/last-added/10
        [HttpGet("last-added/{count}")]
        public IActionResult GetLastAdded(int count)
        {
            var values = _productService.GetLastAddedProducts(count);
            return Ok(values);
        }

        // Vitrin Ürünleri (Çok Satanlar vs.)
        // GET: api/products/showcase/0 (Bestseller)
        [HttpGet("showcase/{displayType}")]
        public IActionResult GetShowcase(DisplayType displayType)
        {
            var values = _productService.GetShowcaseProducts(displayType);
            return Ok(values);
        }

        [HttpPost("add-to-showcase")]
        public IActionResult AddToShowcase(CreateProductDisplayDto dto)
        {
            try
            {
                _productService.AddProductToShowcase(dto);
                return Ok("Ürün vitrine başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Sayfalama (Pagination)
        // GET: api/products/paged?page=1&size=12
        [HttpGet("paged")]
        public IActionResult GetPaged([FromQuery] int page = 1, [FromQuery] int size = 12)
        {
            var values = _productService.GetProductsWithPaging(page, size);
            return Ok(values);
        }

        // =========================================================================
        // 4. ADMİN ÖZEL İŞLEMLERİ
        // =========================================================================

        // Hızlı Stok Güncelleme
        // PUT: api/products/update-stock
        [HttpPut("update-stock")]
        public IActionResult UpdateStock(int productId, decimal newQuantity)
        {
            try
            {
                _productService.UpdateStock(productId, newQuantity);
                return Ok("Stok güncellendi.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Aktif/Pasif Yapma
        // GET: api/products/toggle-status/5
        [HttpGet("toggle-status/{id}")]
        public IActionResult ToggleStatus(int id)
        {
            _productService.ToggleProductStatus(id);
            return Ok("Ürün durumu değiştirildi.");
        }

        [HttpGet("stock-history/{id}")]
        public IActionResult GetStockHistory(int id)
        {
            var values = _productService.GetStockHistory(id);
            return Ok(values);
        }
    }
}