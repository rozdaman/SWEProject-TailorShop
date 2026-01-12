using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Abstract
{
    // Generic yapıya "ProductDisplay" tablosuyla çalışacağımızı söylüyoruz.
    public interface IProductDisplayDal : IGenericDal<ProductDisplay>
    {
        // Şu an için özel bir metot (örn: GetActiveDisplays) yoksa burası boş kalabilir.
        // Ekleme, Silme, Güncelleme gibi metotlar IGenericDal'dan miras geliyor.
    }
}