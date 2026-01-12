using Tailor.DTO.DTOs.CartDtos;

namespace Tailor.DTO.DTOs.ShoppingCartItem
{
    // Alias to reuse Cart DTOs but in namespace expected by MapProfile
    public class ResultCartItemDto : CartItemDetailDto { }
    // AddCartItemDto is already in CartDtos, MapProfile uses it from there? 
    // MapProfile writes: using Tailor.DTO.DTOs.ShoppingCartItem;
    // And CreateMap<ShoppingCartItem, AddCartItemDto>
    // AddCartItemDto might be expected in this namespace too?
    // Let's assume MapProfile imports CartDtos too (it does: using Tailor.DTO.DTOs.CartDtos; logic check later).
}
