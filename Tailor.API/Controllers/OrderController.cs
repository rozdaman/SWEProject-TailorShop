using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tailor.Business.Abstract;
using Tailor.DTO.DTOs.OrderDtos;

namespace Tailor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Sipariş işlemleri için giriş şart (Misafir hariç, onu sonra açacağız)
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // YARDIMCI METOT: Token'dan UserId'yi çözer
        private int GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new Exception("Kullanıcı kimliği doğrulanamadı.");
        }

        // 1. SİPARİŞ OLUŞTUR (CHECKOUT)
        [HttpPost("create")]
        public IActionResult CreateOrder(CreateOrderDto dto)
        {
            try
            {
                var userId = GetUserId();
                _orderService.CreateOrder(userId, dto);
                return Ok(new { message = "Siparişiniz başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // 2. SİPARİŞ GEÇMİŞİM
        [HttpGet("my-orders")]
        public IActionResult GetMyOrders()
        {
            var userId = GetUserId();
            var orders = _orderService.GetMyOrders(userId);
            return Ok(orders);
        }

        // 3. SİPARİŞ DETAYI
        [HttpGet("detail/{id}")]
        public IActionResult GetOrderDetail(int id)
        {
            try
            {
                // Güvenlik: Kullanıcı başkasının siparişini görmesin diye burada kontrol eklenebilir.
                var order = _orderService.GetOrderDetails(id);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}