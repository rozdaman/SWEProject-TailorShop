using System;

namespace Tailor.Entity.Entities
{
    public class Testimonial
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public AppUser User { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
    }
}
