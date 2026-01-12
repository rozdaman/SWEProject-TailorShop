using System;

namespace Tailor.DTO.DTOs.BlogDtos
{
    public class CreateBlogDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class UpdateBlogDto : CreateBlogDto
    {
        public int Id { get; set; }
    }

    public class ResultBlogDto : UpdateBlogDto
    {
        public DateTime CreatedAt { get; set; }
    }
    
    public class ResultBlogListDto : ResultBlogDto { }
}
