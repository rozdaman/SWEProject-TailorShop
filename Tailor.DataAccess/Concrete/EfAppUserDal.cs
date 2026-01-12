using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.DataAccess.Context;
using Tailor.DataAccess.Repositories;

namespace Tailor.DataAccess.Concrete
{
    public class EfAppUserDal : GenericRepository<Tailor.Entity.Entities.AppUser>, Tailor.DataAccess.Abstract.IAppUserDal
    {
        public EfAppUserDal(ApplicationDbContext context) : base(context)
        {
        }
    }
}
