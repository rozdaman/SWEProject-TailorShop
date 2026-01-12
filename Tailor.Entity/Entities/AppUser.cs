using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tailor.Entity.Entities
{
    public class AppUser: IdentityUser<int>
    {
        //IdentityUser'dan gelen ek özelliklere ek olarak kendi özelliklerimiz
        public DateTime RegistreationDate { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        //Diğer varlıklarla olan ilişkiler için navigasyon özellikleri
        public ICollection<Order> Orders { get; set; }
        public ICollection<Address> Addresses { get; set; }
        public ICollection<Testimonial> Testimonials { get; set; }
        public ShoppingCart ShoppigCard { get; set; }
        public ICollection<SupportTicket> SupportTickets { get; set; }//help desk için temel oluşturacak olan classdır.

    }
}
