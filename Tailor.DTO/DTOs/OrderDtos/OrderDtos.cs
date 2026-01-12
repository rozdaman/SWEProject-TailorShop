using System;
using System.Collections.Generic;
using Tailor.Entity.Entities.Enums;
using Tailor.Entity.Entities; // for Address if needed, or AddressDto

namespace Tailor.DTO.DTOs.OrderDtos
{
    public class CreateOrderDto
    {
        public int ShippingAddressId { get; set; }
        public int BillingAddressId { get; set; }
        public string PaymentMethod { get; set; } // "CreditCard", "Transfer"
        public string CardHolderName { get; set; }
        public string CardNumber { get; set; }
        public string ExpireDate { get; set; }
        public string Cvv { get; set; }
        public System.Collections.Generic.List<int> SelectedShoppingCartItemIds { get; set; }
    }

    public class ResultOrderListDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public int TotalItemCount { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; } // for Admin list
    }

    public class ResultOrderDto
    {
        public int OrderId { get; set; } // Renamed to OrderId to match entity
        public int Id { get { return OrderId; } set { OrderId = value; } } // Compat alias just in case
        
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; } // Or OrderStatus enum
        
        public Tailor.DTO.DTOs.AddressDtos.ResultAddressDto ShippingAddress { get; set; }
        public Tailor.DTO.DTOs.AddressDtos.ResultAddressDto BillingAddress { get; set; }

        public List<ResultOrderItemDto> OrderItems { get; set; }
    }

    public class ResultOrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
