using Tailor.DataAccess.Abstract;
using Tailor.DataAccess.Context;
using Tailor.DataAccess.Repositories;
using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Concrete
{
    public class EfShippingDal : GenericRepository<Shipping>, IShippingDal
    {
        private readonly ApplicationDbContext _context;

        public EfShippingDal(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public Shipping GetShippingByOrderId(int orderId)
        {
            return _context.Shippings.FirstOrDefault(x => x.OrderId == orderId);
        }
    }
}