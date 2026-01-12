using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.DTO.DTOs.ProductDtos;
using Tailor.Entity.Entities;

namespace Tailor.Business.Abstract
{
    public interface IFavoriteService : IGenericService<Favorite>
    {
        List<ResultProductListDto> GetUserFavorites(int userId);
        void AddToFavorites(int userId, int productId);
        void RemoveFromFavorites(int userId, int productId);
        bool IsInFavorites(int userId, int productId);
    }
}
