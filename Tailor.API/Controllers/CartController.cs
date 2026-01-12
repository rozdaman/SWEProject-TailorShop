using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Tailor.Business.Abstract;
using Tailor.DTO.DTOs.CartDtos;

namespace Tailor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // =================================================================================
        // YARDIMCI METOTLAR
        // =================================================================================

        private int? GetUserId()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated) return null;

            var claim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier) ??
                        User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub) ??
                        User.Claims.FirstOrDefault(x => x.Type == "id" || x.Type.Contains("nameidentifier"));

            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        private string? GetSessionId()
        {
            // Frontend'den (Node.js/JS) gelen session ID'yi oku
            if (Request.Headers.TryGetValue("X-Session-Id", out var sessionId)) 
            {
                return sessionId.ToString();
            }

            // Fallback (Sadece geliştirme aşamasında lazım olabilir)
            return "misafir_test_ilhan";
        }

        // =================================================================================
        // ENDPOINTLER
        // =================================================================================

        // 1. SEPETİ GETİR
        [HttpGet]
        public IActionResult GetMyCart()
        {
            var userId = GetUserId();
            var sessionId = GetSessionId();

            if (userId == null && string.IsNullOrEmpty(sessionId))
                return Ok(new ShoppingCartDto());

            var cart = _cartService.GetCart(userId, sessionId);
            return Ok(cart);
        }

        // 2. SEPETE ÜRÜN EKLE
        [HttpPost("add")]
        public IActionResult AddToCart(AddCartItemDto dto)
        {
            try
            {
                var userId = GetUserId();
                var sessionId = GetSessionId();

                if (userId == null && string.IsNullOrEmpty(sessionId))
                    return BadRequest("Oturum bilgisi bulunamadı.");

                _cartService.AddToCart(userId, sessionId, dto);
                return Ok(new { message = "Ürün sepete başarıyla eklendi." });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(new { error = errorMessage });
            }
        }

        // 3. MİKTAR GÜNCELLE (EKSİK OLAN KISIM)
        [HttpPut("update-quantity")]
        public IActionResult UpdateQuantity(UpdateCartItemDto dto)
        {
            try
            {
                var userId = GetUserId();
                var sessionId = GetSessionId();

                if (userId == null && string.IsNullOrEmpty(sessionId))
                    return BadRequest("Oturum bilgisi bulunamadı.");

                _cartService.UpdateQuantity(userId, sessionId, dto);
                return Ok(new { message = "Sepet güncellendi." });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(new { error = errorMessage });
            }
        }

        // 4. TEK BİR ÜRÜNÜ SİL (EKSİK OLAN KISIM)
        [HttpDelete("remove/{cartItemId}")]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            try
            {
                var userId = GetUserId();
                var sessionId = GetSessionId();

                if (userId == null && string.IsNullOrEmpty(sessionId))
                    return BadRequest("Oturum bilgisi bulunamadı.");

                _cartService.RemoveFromCart(userId, sessionId, cartItemId);
                return Ok(new { message = "Ürün sepetten silindi." });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(new { error = errorMessage });
            }
        }

        // 5. SEPETİ KOMPLE BOŞALT (DÜZELTİLMİŞ HALİ)
        [HttpDelete("clear")]
        public IActionResult ClearCart()
        {
            try
            {
                var userId = GetUserId();
                var sessionId = GetSessionId();

                if (userId == null && string.IsNullOrEmpty(sessionId))
                    return BadRequest("Oturum bilgisi bulunamadı.");

                // Artık CartId değil, userId/sessionId gönderiyoruz
                _cartService.ClearCart(userId, sessionId);

                return Ok(new { message = "Sepet tamamen boşaltıldı." });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(new { error = errorMessage });
            }
        }

        // Debug Amaçlı (Token'ı kontrol etmek için)
        [HttpGet("debug-claims")]
        public IActionResult DebugClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var isAuthenticated = User.Identity?.IsAuthenticated;

            return Ok(new
            {
                IsAuthenticated = isAuthenticated,
                Claims = claims
            });
        }
    }
}