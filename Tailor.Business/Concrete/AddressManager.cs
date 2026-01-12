using AutoMapper;
using Tailor.Business.Abstract;
using Tailor.DataAccess.Abstract;
using Tailor.DTO.DTOs.AddressDtos;
using Tailor.Entity.Entities;

namespace Tailor.Business.Concrete
{
    public class AddressManager : GenericManager<Address>, IAddressService
    {
        private readonly IAddressDal _addressDal;
        private readonly IMapper _mapper;

        public AddressManager(IAddressDal addressDal, IMapper mapper) : base(addressDal)
        {
            _addressDal = addressDal;
            _mapper = mapper;
        }

        // 1. ADRES EKLEME
        public void CreateAddress(int userId, CreateAddressDto dto)
        {
            var address = _mapper.Map<Address>(dto);
            address.UserId = userId; // Güvenlik: User ID dışarıdan değil, Token'dan gelir

            // Eğer kullanıcının hiç adresi yoksa, ilk eklenen otomatik "Varsayılan" olur.
            var userAddresses = _addressDal.GetAddressesByUserId(userId);
            if (!userAddresses.Any())
            {
                address.IsDefault = true;
            }
            // Eğer yeni adres "Varsayılan" olarak geldiyse, diğerlerini boşa düşür
            else if (dto.IsDefault)
            {
                foreach (var item in userAddresses)
                {
                    item.IsDefault = false;
                    _addressDal.Update(item);
                }
            }

            _addressDal.Add(address);
        }

        // 2. ADRES GÜNCELLEME
        public void UpdateAddress(int userId, UpdateAddressDto dto)
        {
            var address = _addressDal.GetById(dto.Id);

            // Güvenlik: Başkasının adresini güncelleyemesin
            if (address == null || address.UserId != userId)
                throw new Exception("Adres bulunamadı veya bu işlem için yetkiniz yok.");

            // Alanları güncelle (Mapper yerine manuel kontrol daha güvenli olabilir ama Mapper pratik)
            // Ancak ID ve UserId değişmemeli, o yüzden sadece gerekli alanları mapliyoruz veya el ile atıyoruz.

            address.Title = dto.Title;
            address.FullAddress = dto.FullAddress;
            address.City = dto.City;
            address.District = dto.District;
            address.ZipCode = dto.ZipCode;

            // address.State = dto.State; // Entity'de State varsa aç

            // Varsayılan adres mantığı
            if (dto.IsDefault && !address.IsDefault)
            {
                // Bu adres varsayılan yapıldıysa, diğerlerinin varsayılan özelliğini al
                var otherAddresses = _addressDal.GetAddressesByUserId(userId);
                foreach (var item in otherAddresses)
                {
                    item.IsDefault = false;
                    _addressDal.Update(item);
                }
                address.IsDefault = true;
            }
            else
            {
                // Varsayılan değilse normal güncelle
                address.IsDefault = dto.IsDefault;
            }

            _addressDal.Update(address);
        }

        // 3. ADRES SİLME
        public void DeleteAddress(int userId, int addressId)
        {
            var address = _addressDal.GetById(addressId);

            // Güvenlik Kontrolü
            if (address == null || address.UserId != userId)
                throw new Exception("Adres silinemedi.");

            _addressDal.Delete(address);
        }

        // 4. LİSTELEME
        public List<ResultAddressDto> GetUserAddresses(int userId)
        {
            var values = _addressDal.GetAddressesByUserId(userId);
            return _mapper.Map<List<ResultAddressDto>>(values);
        }

        // 5. TEK ADRES GETİRME
        public ResultAddressDto GetAddressById(int userId, int addressId)
        {
            var address = _addressDal.GetById(addressId);

            if (address == null || address.UserId != userId)
                throw new Exception("Adres bulunamadı.");

            return _mapper.Map<ResultAddressDto>(address);
        }
    }
}