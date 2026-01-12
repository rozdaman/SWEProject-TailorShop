using System;

namespace Tailor.DTO.DTOs.SupportTicketDtos
{
    public class CreateSupportTicketDto
    {
        public int UserId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public class ResultSupportTicketDto : CreateSupportTicketDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }
}
