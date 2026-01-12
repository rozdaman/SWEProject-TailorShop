using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tailor.DTO.DTOs.AddressDtos;
using Tailor.Entity.Entities;

namespace Tailor.Business.Abstract
{
    public interface IAddressService : IGenericService<Address>
    {
        void CreateAddress(int userId, CreateAddressDto dto);
        void UpdateAddress(int userId, UpdateAddressDto dto);
        void DeleteAddress(int userId, int addressId); // Güvenlik için UserId de alıyoruz
        List<ResultAddressDto> GetUserAddresses(int userId);
        ResultAddressDto GetAddressById(int userId, int addressId);
    }
}