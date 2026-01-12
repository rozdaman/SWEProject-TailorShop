using Microsoft.AspNetCore.Mvc;
using Tailor.Business.Abstract;
using Tailor.DTO.DTOs.PaymentDtos;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("pay")]
    public IActionResult Pay(CreatePaymentDto dto)
    {
        var success = _paymentService.ProcessPayment(dto);
        if (success) return Ok("Ödeme başarılı.");
        return BadRequest("Ödeme alınamadı.");
    }
}