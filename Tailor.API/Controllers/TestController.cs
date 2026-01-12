using Microsoft.AspNetCore.Mvc;
using Tailor.Business.Abstract;
using Tailor.DTO.DTOs.CategoryDtos;
using Tailor.DTO.DTOs.ProductDtos;

namespace Tailor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public TestController(ICategoryService categoryService, IProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
        }

        [HttpGet("seed")]
        public IActionResult Seed()
        {
            Console.WriteLine("Seed metodu tetiklendi.");
            try
            {
                // 1. Kategori Ekle
                var catDto = new CreateCategoryDto 
                { 
                    Name = "Kıyafetler",
                    ImageUrl = "dummy.jpg" 
                };
                _categoryService.CreateCategory(catDto);

                // 2. Ürün Ekle (Kategori ID'sini 1 varsayıyoruz veya çekiyoruz)
                // Gerçek senaryoda var olanı kontrol edebiliriz ama boş DB varsayımıyla devam.
                
                var prodDto = new CreateProductDto
                {
                    Name = "Test Ceket",
                    // ProductCode removed as it is not in DTO
                    Price = 1500,
                    // StockQuantity removed (handled by stock service or default?)
                    Description = "Test ürünü",
                    CategoryId = 1, 
                    ProductType = Tailor.Entity.Entities.Enums.ProductType.Fabric,
                    ImageUrl = "https://via.placeholder.com/300"
                };

                _productService.CreateProduct(prodDto);

                return Ok("Seed Başarılı! Kontrol edebilirsiniz.");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"SEED HATASI: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                // Hatayı ekrana bas
                return BadRequest($"HATA OLUŞTU: {ex.Message} \n {ex.InnerException?.Message} \n {ex.StackTrace}");
            }
        }
    }
}
