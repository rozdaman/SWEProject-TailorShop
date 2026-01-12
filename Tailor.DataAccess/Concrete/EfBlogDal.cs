using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.DataAccess.Context;
using Tailor.DataAccess.Repositories;

namespace Tailor.DataAccess.Concrete
{
    public class EfBlogDal : GenericRepository<Tailor.Entity.Entities.Blog>, Tailor.DataAccess.Abstract.IBlogDal
    {
        public EfBlogDal(ApplicationDbContext context) : base(context)
        {
        }
    }
}
