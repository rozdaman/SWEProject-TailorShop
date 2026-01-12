using System;

namespace Tailor.DTO.DTOs.CategoryDtos
{
    public class CreateCategoryDto
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }

    public class UpdateCategoryDto : CreateCategoryDto
    {
        public int Id { get; set; }
    }

    public class ResultCategoryDto : UpdateCategoryDto
    {
        // ...
    }
}
