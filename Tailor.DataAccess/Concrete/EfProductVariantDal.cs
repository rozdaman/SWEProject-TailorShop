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
    public class EfProductVariantDal : GenericRepository<ProductVariant>, IProductVariantDal
    {
        public EfProductVariantDal(ApplicationDbContext context) : base(context)
        {
        }
    }
}
