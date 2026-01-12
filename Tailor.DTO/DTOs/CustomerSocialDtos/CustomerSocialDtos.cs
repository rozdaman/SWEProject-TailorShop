namespace Tailor.DTO.DTOs.CustomerSocialDtos
{
    public class CreateCustomerSocialDto
    {
        public string Platform { get; set; }
        public string Url { get; set; }
    }
    public class UpdateCustomerSocialDto : CreateCustomerSocialDto { public int Id { get; set; } }
    public class ResultCustomerSocialDto : UpdateCustomerSocialDto { }
}
