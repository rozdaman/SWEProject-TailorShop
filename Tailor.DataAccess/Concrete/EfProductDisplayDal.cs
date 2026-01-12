using Tailor.DataAccess.Abstract;
using Tailor.DataAccess.Context;
using Tailor.DataAccess.Repositories; // GenericRepository'nin olduğu yer
using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Concrete
{
    public class EfProductDisplayDal : GenericRepository<ProductDisplay>, IProductDisplayDal
    {
        // Constructor (Yapıcı Metot)
        // Veritabanı bağlantısını (context) alıp, üst sınıf olan GenericRepository'e (base) gönderiyoruz.
        public EfProductDisplayDal(ApplicationDbContext context) : base(context)
        {
        }
    }
}