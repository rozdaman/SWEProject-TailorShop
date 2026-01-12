using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.DataAccess.Context;
using Tailor.DataAccess.Repositories;

namespace Tailor.DataAccess.Concrete
{
    public class EfCategoryDal : GenericRepository<Tailor.Entity.Entities.Category>, Tailor.DataAccess.Abstract.ICategoryDal
    {
        public EfCategoryDal(ApplicationDbContext context) : base(context)
        {
        }
    }
}
