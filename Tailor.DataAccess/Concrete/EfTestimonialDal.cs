using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.DataAccess.Repositories;

namespace Tailor.DataAccess.Concrete
{
    public class EfTestimonialDal : GenericRepository<Tailor.Entity.Entities.Testimonial>, Tailor.DataAccess.Abstract.ITestimonialDal
    {
        public EfTestimonialDal(Tailor.DataAccess.Context.ApplicationDbContext context) : base(context)
        {
        }
    }
    
    
}
