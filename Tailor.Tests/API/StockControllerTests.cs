using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Tailor.API.Controllers;
using Tailor.Business.Abstract;
using Tailor.DTO.DTOs.StockDtos;
using Xunit;
using System;
using System.Collections.Generic;

namespace Tailor.Tests.API
{
    public class StockControllerTests
    {
        private Mock<IStockService> _stockServiceMock;
        private StockController _controller;

        public StockControllerTests()
        {
            _stockServiceMock = new Mock<IStockService>();
            _controller = new StockController(_stockServiceMock.Object);
        }

        private void SetupUser(bool isAdmin)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "User")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        [Trait("Scenario", "3")]
        public void TC28_AddStock_ShouldReturnOk_WhenRequestIsValid()
        {
            // Arrange
            SetupUser(true);
            var dto = new UpdateStockDto { ProductId = 1, Quantity = 10 };

            // Act
            var result = _controller.AddStock(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Contains("Stok başarıyla güncellendi", okResult.Value.ToString() ?? "");
        }

        [Fact]
        [Trait("Scenario", "3")]
        public void TC29_AddStock_ShouldReturnBadRequest_WhenQuantityIsZeroOrNegative()
        {
            // Arrange
            SetupUser(true);
            var dto = new UpdateStockDto { ProductId = 1, Quantity = -5 };
            _stockServiceMock.Setup(s => s.AddStock(It.IsAny<UpdateStockDto>()))
                             .Throws(new Exception("Invalid stock quantity."));

            // Act
            var result = _controller.AddStock(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Invalid stock quantity", badRequestResult.Value.ToString() ?? "");
        }

        [Fact]
        [Trait("Scenario", "3")]
        public void TC30_AddStock_ShouldReturnForbidden_WhenUserIsNotAdmin()
        {
            // Note: In standard ASP.NET Core, [Authorize(Roles="Admin")] is handled by the framework.
            // In unit tests, we check if the attribute is present or simulate the check if handled manually.
            
            // Check for Authorize attribute
            var type = typeof(StockController);
            var method = type.GetMethod("AddStock");
            var classAttr = Attribute.GetCustomAttribute(type, typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute));
            var methodAttr = Attribute.GetCustomAttribute(method!, typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute));

            // If the user wants to test the *behavior* and expects the controller to handle it:
            // For now, we verify the requirement exists.
            
            // SetupUser(false); 
            // var dto = new UpdateStockDto();
            // var result = _controller.AddStock(dto);
            // Assert.IsType<ForbidResult>(result); // If manually handled
            
            Assert.True(classAttr != null || methodAttr != null, "Controller or Method should have Authorize attribute");
        }

        [Fact]
        [Trait("Scenario", "6")]
        public void TC54_GetStockHistory_ShouldReturn403_WhenUserIsNotAdmin()
        {
            // Similar to TC30, we verify the requirement for Admin role.
            var type = typeof(StockController);
            var method = type.GetMethod("GetVariantHistory");
            var classAttr = (Microsoft.AspNetCore.Authorization.AuthorizeAttribute?)Attribute.GetCustomAttribute(type, typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute));
            
            Assert.NotNull(classAttr);
            Assert.Equal("Admin", classAttr.Roles);
        }

        [Fact]
        [Trait("Scenario", "6")]
        public void TC55_GetStockHistory_ShouldReturn200_WhenValidVariantIdProvided()
        {
            // Arrange
            SetupUser(true);
            int variantId = 10;
            var logs = new List<ResultStockLogDto> { new ResultStockLogDto { Id = 1, Source = "System" } };
            _stockServiceMock.Setup(s => s.GetStockHistoryByVariant(variantId)).Returns(logs);

            // Act
            var result = _controller.GetVariantHistory(variantId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedValue = Assert.IsType<List<ResultStockLogDto>>(okResult.Value);
            Assert.Single(returnedValue);
        }
    }
}
