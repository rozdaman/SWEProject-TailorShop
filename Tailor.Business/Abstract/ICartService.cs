using Tailor.DTO.DTOs.CartDtos;

namespace Tailor.Business.Abstract
{
    public interface ICartService
    {
        ShoppingCartDto GetCart(int? userId, string? sessionId);

        void AddToCart(int? userId, string? sessionId, AddCartItemDto dto);

        // GÜNCELLEME: Parametreler eklendi
        void RemoveFromCart(int? userId, string? sessionId, int cartItemId);

        // GÜNCELLEME: Parametreler eklendi
        void ClearCart(int? userId, string? sessionId);

        // YENİ METOT: Miktar güncelleme
        void UpdateQuantity(int? userId, string? sessionId, UpdateCartItemDto dto);
        void MergeCarts(string sessionId, int userId);
        void RemoveCartItem(int userId, int shoppingCartItemId);
    }
}