using Tailor.Business.Abstract;
using Tailor.DataAccess.Abstract;

using Tailor.DTO.DTOs.ShipmentDtos;
using Tailor.Entity.Entities;
using Tailor.Entity.Entities.Enums;

namespace Tailor.Business.Concrete
{
    public class LogisticsManager : ILogisticsService // Interface'i oluşturmayı unutma
    {
        private readonly IShipmentDal _shipmentDal;
        private readonly IOrderDal _orderDal;

        public LogisticsManager(IShipmentDal shipmentDal, IOrderDal orderDal)
        {
            _shipmentDal = shipmentDal;
            _orderDal = orderDal;
        }

        // 1. KARGOYA VER (Admin)
        public void CreateShipment(CreateShipmentDto dto)
        {
            var order = _orderDal.GetById(dto.OrderId);
            if (order == null) throw new Exception("Sipariş bulunamadı.");

            // Shipment kaydı oluştur
            var shipment = new Shipment
            {
                OrderId = dto.OrderId,
                Carrier = dto.Carrier,
                TrackingNumber = dto.TrackingNumber,
                ShippedDate = DateTime.Now,
                Status = ShipmentStatus.Shipped
            };

            _shipmentDal.Add(shipment);

            // Siparişin durumunu da güncelle
            order.Status = OrderStatus.Shipped;
            _orderDal.Update(order);
        }

        // 2. KARGO TAKİP (Müşteri)
        public ResultShipmentDto GetTrackingInfo(int orderId)
        {
            // Bir siparişin en son shipment kaydını bul
            var shipment = _shipmentDal.GetListByFilter(x => x.OrderId == orderId)
                                       .OrderByDescending(x => x.ShippedDate)
                                       .FirstOrDefault();

            if (shipment == null) return null;

            return new ResultShipmentDto
            {
                Carrier = shipment.Carrier,
                TrackingNumber = shipment.TrackingNumber,
                ShippedDate = shipment.ShippedDate,
                Status = shipment.Status.ToString()
            };
        }
    }
}