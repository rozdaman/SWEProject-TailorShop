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
    public class EfStockDal : GenericRepository<Stock>, IStockDal
    {
        private readonly ApplicationDbContext _context;

        public EfStockDal(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public Stock? GetByProductId(int productId)
        {
            return _context.Stocks.FirstOrDefault(x => x.ProductId == productId);
        }
    }
}
