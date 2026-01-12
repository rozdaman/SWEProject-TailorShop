using Tailor.DataAccess.Abstract;
using Tailor.DataAccess.Context;
using Tailor.DataAccess.Repositories;
using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Concrete
{
    public class EfPaymentDal : GenericRepository<Payment>, IPaymentDal
    {
        private readonly ApplicationDbContext _context;

        public EfPaymentDal(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public Payment? GetPaymentByOrderId(int orderId)
        {
            return _context.Payments.FirstOrDefault(x => x.OrderId == orderId);
        }
    }
}