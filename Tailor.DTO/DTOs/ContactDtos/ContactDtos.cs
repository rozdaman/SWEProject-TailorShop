using System;

namespace Tailor.DTO.DTOs.ContactDtos
{
    public class CreateContactDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public class ResultContactDto : CreateContactDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
