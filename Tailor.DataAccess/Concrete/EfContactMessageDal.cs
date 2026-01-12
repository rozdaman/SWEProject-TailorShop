using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.DataAccess.Context;
using Tailor.DataAccess.Repositories;

namespace Tailor.DataAccess.Concrete
{
    public class EfContactMessageDal : GenericRepository<Tailor.Entity.Entities.ContactMessage>, Tailor.DataAccess.Abstract.IContactMessageDal
    {
        public EfContactMessageDal(ApplicationDbContext context) : base(context)
        {
        }
    
    }
}
