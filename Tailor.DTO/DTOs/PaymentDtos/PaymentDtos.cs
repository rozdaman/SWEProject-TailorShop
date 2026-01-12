using System;

namespace Tailor.DTO.DTOs.PaymentDtos
{
    public class CreatePaymentDto
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string CardHolderName { get; set; }
        public string CardNumber { get; set; }
        public string ExpireDate { get; set; }
        public string Cvv { get; set; }
    }
}
