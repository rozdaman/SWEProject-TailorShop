using System;

namespace Tailor.DTO.DTOs.AddressDtos
{
    public class CreateAddressDto
    {
        public string Title { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string FullAddress { get; set; }
        public string ZipCode { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UpdateAddressDto : CreateAddressDto
    {
        public int Id { get; set; }
    }

    public class ResultAddressDto : CreateAddressDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsDefault { get; set; }
    }
}
