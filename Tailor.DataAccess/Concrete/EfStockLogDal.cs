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
    public class EfStockLogDal : GenericRepository<StockLog>, IStockLogDal
    {
        public EfStockLogDal(ApplicationDbContext context) : base(context)
        {
        }
    }
}
