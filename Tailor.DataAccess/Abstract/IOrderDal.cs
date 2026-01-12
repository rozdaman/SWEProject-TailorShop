using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Abstract
{
    public interface IOrderDal : IGenericDal<Order>
    {
        // Kullanıcının sipariş geçmişini getir
        List<Order> GetOrdersByUserId(int userId);

        // Tek bir siparişi tüm detaylarıyla (Item, Address vb.) getir
        Order? GetOrderWithDetails(int orderId);

        // Tüm siparişleri detaylarıyla getir (Admin için)
        List<Order> GetAllOrdersWithDetails();
    }
}
