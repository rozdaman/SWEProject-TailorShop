using System;

namespace Tailor.DTO.DTOs.AuthDtos // Assuming namespace based on folder
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTime ExpireDate { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }

    public class TokenResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
