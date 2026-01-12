using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tailor.Business.Abstract;
using Tailor.Business.Tools;
using Tailor.DTO.DTOs.AuthDtos;

using Tailor.Entity.Entities; // AppUser ve AppRole burada

namespace Tailor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ICartService _cartService;

        public AuthController(
           UserManager<AppUser> userManager,
           SignInManager<AppUser> signInManager,
           RoleManager<AppRole> roleManager,
           IConfiguration configuration,
           ICartService cartService) // <--- Constructor'a ekle
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _cartService = cartService; // <--- Atamayı yap
        }
        // 1. KAYIT OLMA (REGISTER) - GÜNCELLENMİŞ HALİ
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            // 1. Şifre Kontrolü
            if (registerDto.Password != registerDto.ConfirmPassword)
                return BadRequest("Şifreler uyuşmuyor.");

            // 2. Kullanıcı Nesnesini Oluştur
            var user = new AppUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                Name = registerDto.FirstName,
                Surname = registerDto.LastName
            };

            // 3. Kullanıcıyı Veritabanına Kaydet
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                // --- ESKİ KOD: ROL ATAMA İŞLEMİ ---
                // "Member" rolü yoksa oluştur ve kullanıcıya ata
                if (!await _roleManager.RoleExistsAsync("Member"))
                {
                    await _roleManager.CreateAsync(new AppRole { Name = "Member" });
                }

                await _userManager.AddToRoleAsync(user, "Member");
                // ----------------------------------

                // --- YENİ KOD: SEPET BİRLEŞTİRME (MERGE) ---
                // Kullanıcı başarıyla oluştu. Şimdi misafir sepeti var mı diye bakıp birleştirelim.

                // Oluşan kullanıcının ID'sini garantiye almak için tekrar çekiyoruz
                var createdUser = await _userManager.FindByEmailAsync(registerDto.Email);
                var sessionId = GetSessionIdFromHeader();

                // Eğer header'da SessionId varsa ve kullanıcı oluştuysa birleştir
                if (!string.IsNullOrEmpty(sessionId) && createdUser != null)
                {
                    try
                    {
                        _cartService.MergeCarts(sessionId, createdUser.Id);
                    }
                    catch (Exception ex)
                    {
                        // Sepet birleştirme hatası olursa kayıt iptal olmasın, sadece loglanabilir.
                        // Şimdilik sessizce devam ediyoruz.
                        Console.WriteLine($"Sepet birleştirme hatası: {ex.Message}");
                    }
                }
                // -------------------------------------------

                return Ok("Kullanıcı başarıyla oluşturuldu.");
            }

            return BadRequest(result.Errors);
        }

        // YARDIMCI METOT: Header'dan SessionId okuma
        private string GetSessionIdFromHeader()
        {
            /* if (Request.Headers.TryGetValue("X-Session-Id", out var sessionId))
             {
                 return sessionId.ToString();
             }
             return null;*/
            return "misafir_test_ilhan";
        }

        // 2. GİRİŞ YAPMA (LOGIN)
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null) return BadRequest("Kullanıcı bulunamadı.");

            // Şifre kontrolü
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (result.Succeeded)
            {
                // Kullanıcının rollerini çek
                var roles = await _userManager.GetRolesAsync(user);

                // Token üret
                var tokenResult = JwtTokenGenerator.GenerateToken(user.Email, user.Id, user.UserName, roles.ToList(), _configuration);
                // --- KRİTİK EKLEME: SEPET BİRLEŞTİRME ---
                var sessionId = GetSessionIdFromHeader();
                if (!string.IsNullOrEmpty(sessionId))
                {
                    // "Bu session'daki sepeti, bu User ID'ye aktar"
                    _cartService.MergeCarts(sessionId, user.Id);
                }
                return Ok(tokenResult);
            }

            return Unauthorized("Şifre hatalı.");
        }

        // 3. KULLANICI LİSTESİ (Sadece Admin Görebilir)
        [HttpGet("user-list")]
        // [Authorize(Roles = "Admin")] // Test ederken burayı kapalı tutabilirsin, sonra aç
        public async Task<IActionResult> GetUserList()
        {
            var users = await _userManager.Users.ToListAsync();
            var userListDtos = new List<UserListDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userListDtos.Add(new UserListDto
                {
                    Id = user.Id,
                    NameSurname = user.Name + " " + user.Surname,
                    UserName = user.UserName,
                    Roles = roles.ToList()
                });
            }
            return Ok(userListDtos);
        }

        // 4. ROL ATAMA (ASSIGN ROLE)
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(AssignRoleDto assignRoleDto)
        {
            var user = await _userManager.FindByIdAsync(assignRoleDto.UserId.ToString());
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            var role = await _roleManager.FindByIdAsync(assignRoleDto.RoleId.ToString());
            if (role == null) return NotFound("Rol bulunamadı. Önce rolü oluşturun.");

            if (assignRoleDto.RoleExist)
            {
                // Rolü ver
                await _userManager.AddToRoleAsync(user, role.Name);
            }
            else
            {
                // Rolü geri al
                await _userManager.RemoveFromRoleAsync(user, role.Name);
            }

            return Ok("Rol güncelleme başarılı.");
        }

        // 5. YENİ ROL OLUŞTURMA (Helper endpoint)
        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                return BadRequest("Bu rol zaten var.");

            await _roleManager.CreateAsync(new AppRole { Name = roleName });
            return Ok("Rol oluşturuldu.");
        }

        // 6. ŞİFRE DEĞİŞTİRME (Scenario 7)
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            // Claims üzerinden User ID'yi al (JWT için daha güvenilir)
            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("Kullanıcı kimliği bulunamadı.");

            var userId = int.Parse(userIdClaim.Value);
            var user = await _userManager.FindByIdAsync(userId.ToString());
            
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            // Backend doğrulaması
            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
            {
                return BadRequest("Yeni şifreler uyuşmuyor.");
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            if (result.Succeeded)
            {
                Console.WriteLine($"INFO: User {user.Email} successfully changed password.");
                return Ok(new { message = "Şifre başarıyla değiştirildi." });
            }

            // Hata detaylarını incele
            var errors = result.Errors.Select(e => e.Description).ToList();
            if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
            {
                return BadRequest(new { error = "Mevcut şifreniz hatalı." });
            }

            return BadRequest(new { error = string.Join(", ", errors) });
        }
    }
}