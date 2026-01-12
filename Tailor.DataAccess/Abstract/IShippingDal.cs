using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Abstract
{
    public interface IShippingDal : IGenericDal<Shipping>
    {
        // Özel Metot: Siparişin kargo detayını getir
        Shipping GetShippingByOrderId(int orderId);
    }
}