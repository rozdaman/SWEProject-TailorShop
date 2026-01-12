
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tailor.Business.Abstract;
using Tailor.DTO.DTOs.AddressDtos;

namespace Tailor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        // Token'dan User ID okuyan yardımcı metot
        private int GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new Exception("Kullanıcı kimliği doğrulanamadı.");
        }

        // 1. ADRESLERİMİ LİSTELE
        [HttpGet("my-addresses")]
        public IActionResult GetMyAddresses()
        {
            try
            {
                var userId = GetUserId();
                var values = _addressService.GetUserAddresses(userId);
                return Ok(values);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        // 2. YENİ ADRES EKLE
        [HttpPost]
        public IActionResult CreateAddress(CreateAddressDto dto)
        {
            try
            {
                var userId = GetUserId();
                _addressService.CreateAddress(userId, dto);
                return Ok(new { message = "Adres başarıyla eklendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 3. ADRES GÜNCELLE
        [HttpPut]
        public IActionResult UpdateAddress(UpdateAddressDto dto)
        {
            try
            {
                var userId = GetUserId();
                _addressService.UpdateAddress(userId, dto);
                return Ok(new { message = "Adres güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 4. ADRES SİL
        [HttpDelete("{id}")]
        public IActionResult DeleteAddress(int id)
        {
            try
            {
                var userId = GetUserId();
                _addressService.DeleteAddress(userId, id);
                return Ok(new { message = "Adres silindi." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 5. TEK ADRES GETİR (Detay için)
        [HttpGet("{id}")]
        public IActionResult GetAddressById(int id)
        {
            try
            {
                var userId = GetUserId();
                var value = _addressService.GetAddressById(userId, id);
                return Ok(value);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}