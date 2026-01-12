using Tailor.DataAccess.Abstract;
using Tailor.DataAccess.Context;
using Tailor.DataAccess.Repositories;
using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Concrete
{
    public class EfShipmentDal : GenericRepository<Shipment>, IShipmentDal
    {
        private readonly ApplicationDbContext _context;

        public EfShipmentDal(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<Shipment> GetShipmentsByOrderId(int orderId)
        {
            return _context.Shipments
                           .Where(x => x.OrderId == orderId)
                           .OrderByDescending(x => x.ShippedDate)
                           .ToList();
        }
    }
}