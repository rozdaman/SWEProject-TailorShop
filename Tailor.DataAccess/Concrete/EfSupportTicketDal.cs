using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.DataAccess.Context;
using Tailor.DataAccess.Repositories;

namespace Tailor.DataAccess.Concrete
{
    public class EfSupportTicketDal : GenericRepository<Tailor.Entity.Entities.SupportTicket>, Tailor.DataAccess.Abstract.ISupportTicketDal
    {
        public EfSupportTicketDal(ApplicationDbContext context) : base(context)
        {
        }
    }
    
    
}
