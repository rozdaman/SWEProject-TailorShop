using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Abstract
{
    public interface IPaymentDal : IGenericDal<Payment>
    {
        // Özel Metot: Bir siparişin ödemesini bul
        Payment? GetPaymentByOrderId(int orderId);
    }
}