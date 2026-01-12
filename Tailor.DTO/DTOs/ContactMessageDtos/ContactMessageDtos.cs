using System;

namespace Tailor.DTO.DTOs.ContactMessageDtos
{
    public class CreateContactMessageDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public class ResultContactMessageDto : CreateContactMessageDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
