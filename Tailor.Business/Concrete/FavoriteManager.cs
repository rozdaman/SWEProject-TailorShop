using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.Business.Abstract;
using Tailor.DataAccess.Abstract;
using Tailor.DTO.DTOs.ProductDtos;
using Tailor.Entity.Entities;

namespace Tailor.Business.Concrete
{
    public class FavoriteManager : GenericManager<Favorite>, IFavoriteService
    {
        private readonly IFavoriteDal _favoriteDal;
        private readonly IMapper _mapper;

        public FavoriteManager(IFavoriteDal favoriteDal, IMapper mapper) : base(favoriteDal)
        {
            _favoriteDal = favoriteDal;
            _mapper = mapper;
        }

        public void AddToFavorites(int userId, int productId)
        {
            var exists = _favoriteDal.GetByFilter(x => x.UserId == userId && x.ProductId == productId);
            if (exists == null)
            {
                _favoriteDal.Add(new Favorite
                {
                    UserId = userId,
                    ProductId = productId,
                    CreatedAt = DateTime.Now
                });
            }
        }

        public List<ResultProductListDto> GetUserFavorites(int userId)
        {
            var favorites = _favoriteDal.GetListWithRelations(x => x.UserId == userId, "Product", "Product.Category");
            var products = favorites.Select(x => x.Product).ToList();
            return _mapper.Map<List<ResultProductListDto>>(products);
        }

        public bool IsInFavorites(int userId, int productId)
        {
            return _favoriteDal.GetByFilter(x => x.UserId == userId && x.ProductId == productId) != null;
        }

        public void RemoveFromFavorites(int userId, int productId)
        {
            var favorite = _favoriteDal.GetByFilter(x => x.UserId == userId && x.ProductId == productId);
            if (favorite != null)
            {
                _favoriteDal.Delete(favorite);
            }
        }
    }
}
