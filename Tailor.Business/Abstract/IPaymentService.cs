using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.DTO.DTOs.PaymentDtos;

namespace Tailor.Business.Abstract
{
    public interface IPaymentService
    {
        // Ödemeyi gerçekleştirir (Banka entegrasyonu simülasyonu)
        // Başarılı olursa true, başarısız olursa false döner.
        bool ProcessPayment(CreatePaymentDto dto);
    }
}
