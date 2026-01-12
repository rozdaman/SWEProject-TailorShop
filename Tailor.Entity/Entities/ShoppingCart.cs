using System;
using System.Collections.Generic;

namespace Tailor.Entity.Entities
{
    public class ShoppingCart
    {
        public int ShoppingCartId { get; set; }
        public int? UserId { get; set; }
        // public AppUser User { get; set; } // Might cause cycle if not careful, but usually needed
        public string SessionId { get; set; }
        public ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }
    }
}
