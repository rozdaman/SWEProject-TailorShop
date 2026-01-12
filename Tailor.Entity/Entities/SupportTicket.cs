using System;

namespace Tailor.Entity.Entities
{
    public class SupportTicket
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public AppUser User { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool IsResolved { get; set; }
        
        public int? AdminId { get; set; }
        public AppUser Admin { get; set; }
    }
}
