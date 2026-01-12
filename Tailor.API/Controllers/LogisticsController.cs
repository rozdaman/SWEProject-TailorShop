using Microsoft.AspNetCore.Mvc;
using Tailor.Business.Abstract;
using Tailor.DTO.DTOs.ShipmentDtos;

[Route("api/[controller]")]
[ApiController]
public class LogisticsController : ControllerBase
{
    private readonly ILogisticsService _logisticsService;

    public LogisticsController(ILogisticsService logisticsService)
    {
        _logisticsService = logisticsService;
    }

    // Admin: Siparişi Kargola
    [HttpPost("create-shipment")]
    // [Authorize(Roles = "Admin")] // Sadece Admin yapabilir
    public IActionResult CreateShipment(CreateShipmentDto dto)
    {
        _logisticsService.CreateShipment(dto);
        return Ok("Sipariş kargoya verildi ve takip numarası oluşturuldu.");
    }

    // Müşteri: Kargom Nerede?
    [HttpGet("track/{orderId}")]
    public IActionResult TrackOrder(int orderId)
    {
        var result = _logisticsService.GetTrackingInfo(orderId);
        if (result == null) return NotFound("Bu sipariş için henüz kargo bilgisi girilmedi.");
        return Ok(result);
    }
}