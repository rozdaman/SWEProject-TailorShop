using Tailor.DTO.DTOs.CategoryDtos;
using Tailor.Entity.Entities;

namespace Tailor.Business.Abstract
{
    public interface ICategoryService : IGenericService<Category>
    {
        void CreateCategory(CreateCategoryDto createCategoryDto);
        void UpdateCategory(UpdateCategoryDto updateCategoryDto);
    }
}
