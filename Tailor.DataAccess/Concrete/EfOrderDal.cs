using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.DataAccess.Context;
using Tailor.DataAccess.Repositories;
using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Concrete
{
    public class EfOrderDal : GenericRepository<Tailor.Entity.Entities.Order>, Tailor.DataAccess.Abstract.IOrderDal
    {
        private readonly ApplicationDbContext _context;

        public EfOrderDal(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<Order> GetOrdersByUserId(int userId)
        {
            return _context.Orders
                .Where(x => x.UserId == userId)
                .Include(x => x.OrderItems) // Kaç parça olduğunu saymak için lazım
                .OrderByDescending(x => x.OrderDate)
                .ToList();
        }

        public Order? GetOrderWithDetails(int orderId)
        {
            return _context.Orders
                .Include(x => x.OrderItems).ThenInclude(i => i.Product) // Ürün ismini almak için
                .Include(x => x.ShippingAddress)
                .Include(x => x.BillingAddress)
                .Include(x => x.User)
                .FirstOrDefault(x => x.OrderId == orderId);
        }

        public List<Order> GetAllOrdersWithDetails()
        {
            return _context.Orders
                .Include(x => x.OrderItems)
                .Include(x => x.User) // Müşteri adını görmek için
                .OrderByDescending(x => x.OrderDate)
                .ToList();
        }

    }
}
