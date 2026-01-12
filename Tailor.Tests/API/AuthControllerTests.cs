using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Tailor.API.Controllers;
using Tailor.Business.Abstract;
using Tailor.DTO.DTOs.AuthDtos;
using Tailor.Entity.Entities;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tailor.Tests.API
{
    public class AuthControllerTests
    {
        private Mock<UserManager<AppUser>> _userManagerMock;
        private Mock<SignInManager<AppUser>> _signInManagerMock;
        private Mock<RoleManager<AppRole>> _roleManagerMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<ICartService> _cartServiceMock;
        private AuthController _controller;

        public AuthControllerTests()
        {
            // UserManager and SignInManager mocks require complex constructor setups
            var userStoreMock = new Mock<IUserStore<AppUser>>();
            _userManagerMock = new Mock<UserManager<AppUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<AppUser>>();
            _signInManagerMock = new Mock<SignInManager<AppUser>>(_userManagerMock.Object, contextAccessorMock.Object, userPrincipalFactoryMock.Object, null, null, null, null);
            
            var roleStoreMock = new Mock<IRoleStore<AppRole>>();
            _roleManagerMock = new Mock<RoleManager<AppRole>>(roleStoreMock.Object, null, null, null, null);
            
            _configurationMock = new Mock<IConfiguration>();
            _cartServiceMock = new Mock<ICartService>();

            _controller = new AuthController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _roleManagerMock.Object,
                _configurationMock.Object,
                _cartServiceMock.Object
            );
        }

        [Fact]
        [Trait("Scenario", "4")]
        public async Task TC37_Login_ShouldTriggerCartMerge_WhenGuestSessionExists()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@test.com", Password = "Password123!" };
            var user = new AppUser { Id = 1, Email = loginDto.Email, UserName = "testuser" };

            _userManagerMock.Setup(s => s.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(s => s.CheckPasswordSignInAsync(user, loginDto.Password, false))
                             .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _userManagerMock.Setup(s => s.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Member" });

            // JWT token generator needs configuration key
            _configurationMock.Setup(c => c["JwtSettings:Key"]).Returns("ThisIsAVeryLongAndSecretKeyForTestingOnly123!");
            _configurationMock.Setup(c => c["JwtSettings:Issuer"]).Returns("TailorApp");
            _configurationMock.Setup(c => c["JwtSettings:Audience"]).Returns("TailorApp");
            _configurationMock.Setup(c => c["JwtSettings:ExpireDays"]).Returns("7");

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            _cartServiceMock.Verify(s => s.MergeCarts(It.Is<string>(id => !string.IsNullOrEmpty(id)), user.Id), Times.Once);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        [Trait("Scenario", "4")]
        public async Task TC38_MergeCarts_ShouldReturnOk_InLoginFlow()
        {
            // This is essentially same as TC37 but focusing on the return type
            // Arrange
            var loginDto = new LoginDto { Email = "test@test.com", Password = "Password123!" };
            var user = new AppUser { Id = 1, Email = loginDto.Email, UserName = "testuser" };

            _userManagerMock.Setup(s => s.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(s => s.CheckPasswordSignInAsync(user, loginDto.Password, false))
                             .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _userManagerMock.Setup(s => s.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Member" });
            _configurationMock.Setup(c => c["JwtSettings:Key"]).Returns("ThisIsAVeryLongAndSecretKeyForTestingOnly123!");
            _configurationMock.Setup(c => c["JwtSettings:Issuer"]).Returns("TailorApp");
            _configurationMock.Setup(c => c["JwtSettings:Audience"]).Returns("TailorApp");
            _configurationMock.Setup(c => c["JwtSettings:ExpireDays"]).Returns("7");

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        [Trait("Scenario", "7")]
        public void TC58_ChangePassword_ShouldRequireAuthorizedUser()
        {
            // Verify attribute
            var type = typeof(AuthController);
            var method = type.GetMethod("ChangePassword");
            var attr = Attribute.GetCustomAttribute(method!, typeof(AuthorizeAttribute));

            Assert.NotNull(attr);
        }

        [Fact]
        [Trait("Scenario", "7")]
        public async Task TC59_ChangePassword_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var user = new AppUser { Id = 1, UserName = "test" };
            var dto = new ChangePasswordDto { CurrentPassword = "Old", NewPassword = "New" };
            
            _userManagerMock.Setup(s => s.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(user);
            _userManagerMock.Setup(s => s.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
                            .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ChangePassword(dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        [Trait("Scenario", "7")]
        public async Task TC60_ChangePassword_ShouldReturnBadRequest_WhenCurrentPasswordIncorrect()
        {
            // Arrange
            var user = new AppUser { Id = 1 };
            var dto = new ChangePasswordDto { CurrentPassword = "Wrong", NewPassword = "New" };
            
            _userManagerMock.Setup(s => s.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(user);
            _userManagerMock.Setup(s => s.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "PasswordMismatch" }));

            // Act
            var result = await _controller.ChangePassword(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Current password does not match.", badRequest.Value);
        }

        [Fact]
        [Trait("Scenario", "7")]
        public async Task TC61_ChangePassword_ShouldInvokeIdentityUpdate_WithHashedPassword()
        {
            // In unit tests, we verify the call to ChangePasswordAsync. 
            // The framework handles the actual hashing.
            // Arrange
            var user = new AppUser { Id = 1 };
            var dto = new ChangePasswordDto { CurrentPassword = "Old", NewPassword = "NewPassword123!" };
            _userManagerMock.Setup(s => s.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(user);
            _userManagerMock.Setup(s => s.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
                            .ReturnsAsync(IdentityResult.Success);

            // Act
            await _controller.ChangePassword(dto);

            // Assert
            _userManagerMock.Verify(s => s.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword), Times.Once);
        }

        [Fact]
        [Trait("Scenario", "7")]
        public async Task TC62_ChangePassword_ShouldFail_WhenNewPasswordIsTooWeak()
        {
            // Arrange
            var user = new AppUser { Id = 1 };
            var dto = new ChangePasswordDto { CurrentPassword = "Old", NewPassword = "123" };
            
            _userManagerMock.Setup(s => s.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(user);
            _userManagerMock.Setup(s => s.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "PasswordTooShort", Description = "Too short" }));

            // Act
            var result = await _controller.ChangePassword(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<List<IdentityError>>(badRequest.Value);
        }
    }
}
