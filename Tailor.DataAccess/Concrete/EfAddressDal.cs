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
    public class EfAddressDal : GenericRepository<Address>, IAddressDal
    {
        private readonly ApplicationDbContext _context;

        public EfAddressDal(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<Address> GetAddressesByUserId(int userId)
        {
            return _context.Addresses
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.IsDefault) // Varsayılan en üstte
                .ToList();
        }
    }
}