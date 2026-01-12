using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Abstract
{
    public interface IStockDal : IGenericDal<Stock>
    {
        // Ürün ID'sine göre stoğu getiren özel metot
        Stock? GetByProductId(int productId);
    }
}
