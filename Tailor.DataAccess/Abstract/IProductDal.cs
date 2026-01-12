using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Abstract
{
    public interface IProductDal : IGenericDal<Product>
    {
        // YENİ EKLE: Son N ürünü getiren özel metot
        List<Product> GetLastProducts(int count);

    }
}
