using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.DTO.DTOs.ShipmentDtos;

namespace Tailor.Business.Abstract
{
    public interface ILogisticsService
    {
        // Admin: Siparişi kargoya verir ve takip kodu oluşturur
        void CreateShipment(CreateShipmentDto dto);

        // Müşteri: Sipariş ID'sine göre kargonun nerede olduğunu sorgular
        ResultShipmentDto GetTrackingInfo(int orderId);
    }
}
