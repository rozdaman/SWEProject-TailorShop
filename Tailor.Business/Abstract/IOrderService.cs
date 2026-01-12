using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.DTO.DTOs.OrderDtos;

namespace Tailor.Business.Abstract
{
    public interface IOrderService
    {
        void CreateOrder(int userId, CreateOrderDto dto);
        List<ResultOrderListDto> GetMyOrders(int userId);
        ResultOrderDto GetOrderDetails(int orderId);
        
        // Admin için
        List<ResultOrderListDto> GetAllOrders();
        void UpdateOrderStatus(int orderId, Tailor.Entity.Entities.Enums.OrderStatus newStatus);
    }
}
