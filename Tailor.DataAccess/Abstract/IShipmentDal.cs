using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Abstract
{
    public interface IShipmentDal : IGenericDal<Shipment>
    {
        // Özel Metot: Siparişe ait tüm gönderim hareketlerini getir
        List<Shipment> GetShipmentsByOrderId(int orderId);
    }
}