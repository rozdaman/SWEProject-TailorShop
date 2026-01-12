using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Configuration;
using Tailor.Business.Abstract;
using Tailor.DataAccess.Abstract;
using Tailor.DTO.DTOs.PaymentDtos;
using Tailor.Entity.Entities.Enums;

namespace Tailor.Business.Concrete
{
    public class PaymentManager : IPaymentService
    {
        private readonly IPaymentDal _paymentDal;

        public PaymentManager(IPaymentDal paymentDal)
        {
            _paymentDal = paymentDal;
        }

        public bool ProcessPayment(CreatePaymentDto dto)
        {
            // --- MOCK ÖDEME SİMÜLASYONU ---
            // Gerçek hayatta burada Iyzico veya Stripe servisine gidilir.
            // Şimdilik her zaman başarılı kabul ediyoruz.
            // Veritabanına kayıt işlemi OrderManager tarafında yapılıyor (OrderId oluştuktan sonra).
            
            bool bankResult = true; // Bankadan gelen cevap (Simüle)

            return bankResult;
        }
    }
}
