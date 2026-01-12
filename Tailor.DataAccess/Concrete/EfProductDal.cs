using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.DataAccess.Abstract;
using Tailor.DataAccess.Context;
using Tailor.DataAccess.Repositories;
using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Concrete
{
    public class EfProductDal : GenericRepository<Tailor.Entity.Entities.Product>, Tailor.DataAccess.Abstract.IProductDal
    {

        // Context'e erişmemiz lazım, o yüzden Constructor'ı genişletiyoruz
        private readonly ApplicationDbContext _context;

        public EfProductDal(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<Product> GetLastProducts(int count)
        {
            // İŞTE PERFORMANS BURADA:
            // Veritabanına "Bana sadece son 'count' kadarını ver" diyoruz.
            // Include("Category") ekledik ki kategori isimleri de gelsin.

            return _context.Products
                .Include(x => x.Category)            // İlişkiyi dahil et
                .OrderByDescending(x => x.CretaedAt) // Tarihe göre tersten sırala (SQL'de yapar)
                .Take(count)                         // Sadece 'count' kadar al (SQL TOP komutu)
                .ToList();                           // ŞİMDİ veritabanına git ve al.
        }
    }
}
