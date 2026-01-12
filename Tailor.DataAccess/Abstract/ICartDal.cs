using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Abstract
{
    public interface ICartDal : IGenericDal<ShoppingCart>
    {
        // Kullanıcı ID'sine veya Session ID'sine göre sepeti dolu getir
        ShoppingCart? GetCartWithItems(int? userId, string? sessionId);
        // YENİ EKLE: Sepet Kalemini ID ile silme metodu
        void DeleteCartItem(int cartItemId);
    }
}
