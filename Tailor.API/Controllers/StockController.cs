using Microsoft.AspNetCore.Authorization; // Admin yetkisi için
using Microsoft.AspNetCore.Mvc;
using Tailor.Business.Abstract;
using Tailor.DTO.DTOs.StockDtos;

namespace Tailor.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        // 1. TÜM STOKLARI GÖR (Admin)
        [HttpGet("list")]
        public IActionResult GetStockList()
        {
            var values = _stockService.GetStockList();
            return Ok(values);
        }

        // 2. TEK ÜRÜN STOĞU (Admin/Sistem)
        [HttpGet("by-product/{productId}")]
        public IActionResult GetStockByProductId(int productId)
        {
            var value = _stockService.GetStockByProductId(productId);
            return Ok(value);
        }

       [HttpPost("add-stock")]
        public IActionResult AddStock(UpdateStockDto dto)
        {
            try
            {
                _stockService.AddStock(dto);
                return Ok(new { message = "Stok başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                // Gizli hatayı (InnerException) ortaya çıkarıyoruz
                var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(new { error = "Stok güncellendi ama Loglama hatası: " + realError });
            }
        }
        

        // 4. STOK KONTROL (Sepet için Helper Endpoint)
        [HttpGet("check-stock")]
        public IActionResult CheckStock(int productId, decimal quantity)
        {
            bool isAvailable = _stockService.CheckStock(productId, quantity);
            return Ok(new { isAvailable = isAvailable });
        }

        // 5. VARYANT STOK GEÇMİŞİ (Senaryo 6)
        [HttpGet("history/variant/{variantId}")]
        public IActionResult GetVariantHistory(int variantId)
        {
            var values = _stockService.GetStockHistoryByVariant(variantId);
            return Ok(values);
        }
    }
}