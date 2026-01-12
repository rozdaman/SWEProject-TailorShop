using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tailor.Business.Abstract;

namespace Tailor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Favoriler için giriş yapmış olma şartı
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) throw new Exception("Kullanıcı ID bulunamadı.");
            return int.Parse(userIdClaim.Value);
        }

        [HttpGet("my-favorites")]
        public IActionResult GetMyFavorites()
        {
            try
            {
                var userId = GetUserId();
                var favorites = _favoriteService.GetUserFavorites(userId);
                return Ok(favorites);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("toggle/{productId}")]
        public IActionResult ToggleFavorite(int productId)
        {
            try
            {
                var userId = GetUserId();
                if (_favoriteService.IsInFavorites(userId, productId))
                {
                    _favoriteService.RemoveFromFavorites(userId, productId);
                    return Ok(new { message = "Ürün favorilerden çıkarıldı.", isFavorite = false });
                }
                else
                {
                    _favoriteService.AddToFavorites(userId, productId);
                    return Ok(new { message = "Ürün favorilere eklendi.", isFavorite = true });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("check/{productId}")]
        public IActionResult CheckFavorite(int productId)
        {
            try
            {
                var userId = GetUserId();
                var isInFavorites = _favoriteService.IsInFavorites(userId, productId);
                return Ok(new { isFavorite = isInFavorites });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
