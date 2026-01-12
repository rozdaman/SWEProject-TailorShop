using Microsoft.EntityFrameworkCore;
using Tailor.DataAccess.Abstract;
using Tailor.DataAccess.Context;
using Tailor.DataAccess.Repositories;
using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Concrete
{
    public class EfCartDal : GenericRepository<ShoppingCart>, ICartDal
    {
        private readonly ApplicationDbContext _context;

        public EfCartDal(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public ShoppingCart? GetCartWithItems(int? userId, string? sessionId)
        {
            // (Mevcut kodların burada duruyor...)
            var query = _context.ShoppingCards
                .Include(x => x.ShoppingCartItems)
                .ThenInclude(i => i.Product)
                .AsQueryable();

            if (userId.HasValue)
                return query.FirstOrDefault(x => x.UserId == userId);
            else
                return query.FirstOrDefault(x => x.SessionId == sessionId);
        }

        // YENİ METOT: ID'ye göre direkt silme (Performanslı Yöntem)
        public void DeleteCartItem(int cartItemId)
        {
            // 1. Önce silinecek nesneyi bul
            var itemToDelete = _context.ShoppingCartItems.Find(cartItemId);

            // 2. Eğer varsa sil ve kaydet
            if (itemToDelete != null)
            {
                _context.ShoppingCartItems.Remove(itemToDelete);
                _context.SaveChanges();
            }
        }
    }
}