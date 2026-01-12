using System.Collections.Generic;

namespace Tailor.DTO.DTOs.CartDtos
{
    public class ShoppingCartDto
    {
        public int ShoppingCartId { get; set; }
        public int? UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CartItemDetailDto> Items { get; set; }
    }

    public class CartItemDetailDto
    {
        public int ShoppingCartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int? VariantId { get; set; }
        public string VariantName { get; set; } // Örn: "Mavi / Standart"
    }

    public class AddCartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int? VariantId { get; set; }
    }

    public class UpdateCartItemDto
    {
        public int CartItemId { get; set; }
        public int NewQuantity { get; set; }
    }
}
