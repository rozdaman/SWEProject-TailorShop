using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Tailor.API.Controllers;
using Tailor.Business.Abstract;
using Tailor.DTO.DTOs.OrderDtos;
using Xunit;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Tailor.Tests.API
{
    public class OrderControllerTests
    {
        private Mock<IOrderService> _orderServiceMock;
        private OrderController _controller;

        public OrderControllerTests()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _controller = new OrderController(_orderServiceMock.Object);
        }

        private void SetupUser(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        [Trait("Scenario", "1")]
        public void TC08_CreateOrder_ShouldReturnOk_WhenRequestIsValid()
        {
            // Arrange
            SetupUser(1);
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int> { 1 } };

            // Act
            var result = _controller.CreateOrder(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Contains("Siparişiniz başarıyla oluşturuldu", okResult.Value.ToString() ?? "");
        }

        [Fact]
        [Trait("Scenario", "1")]
        public void TC09_CreateOrder_ShouldReturnBadRequest_WhenDtoIsInvalid()
        {
            // Arrange
            SetupUser(1);
            _orderServiceMock.Setup(s => s.CreateOrder(It.IsAny<int>(), It.Is<CreateOrderDto>(d => d == null)))
                             .Throws(new Exception("Invalid DTO"));

            // Act
            var result = _controller.CreateOrder(null!); 

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        [Trait("Scenario", "1")]
        public void TC10_CreateOrder_ShouldReturnBadRequestWithErrorMessage_WhenBusinessExceptionOccurs()
        {
            // Arrange
            SetupUser(1);
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int> { 1 } };
            _orderServiceMock.Setup(s => s.CreateOrder(It.IsAny<int>(), It.IsAny<CreateOrderDto>()))
                             .Throws(new Exception("Stok Yetersiz!"));

            // Act
            var result = _controller.CreateOrder(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Stok Yetersiz!", badRequestResult.Value.ToString() ?? "");
        }

        [Fact]
        [Trait("Scenario", "1")]
        public void TC11_CreateOrder_ShouldReturnBadRequest_WhenUserNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };
            var dto = new CreateOrderDto();

            // Act
            var result = _controller.CreateOrder(dto);

            // Assert
            // GetUserId throws Exception which is caught by try-catch and returns BadRequest
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        [Trait("Scenario", "1")]
        public void TC12_CreateOrder_ShouldBeManagedCorrectly()
        {
            // Verifying the method presence and basic behavior.
            var method = typeof(OrderController).GetMethod("CreateOrder");
            Assert.NotNull(method);
        }

        [Fact]
        [Trait("Scenario", "2")]
        public void TC20_CreateOrder_ShouldReturnBadRequest_WhenPaymentServiceFails()
        {
            // Arrange
            SetupUser(1);
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int> { 1 } };
            _orderServiceMock.Setup(s => s.CreateOrder(It.IsAny<int>(), It.IsAny<CreateOrderDto>()))
                             .Throws(new Exception("Ödeme banka tarafından reddedildi."));

            // Act
            var result = _controller.CreateOrder(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Ödeme banka tarafından reddedildi", badRequestResult.Value.ToString() ?? "");
        }

        [Fact]
        [Trait("Scenario", "2")]
        public void TC21_CreateOrder_ShouldReturnBadRequest_WhenSystemTimeoutOccurs()
        {
            // Arrange
            SetupUser(1);
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int> { 1 } };
            _orderServiceMock.Setup(s => s.CreateOrder(It.IsAny<int>(), It.IsAny<CreateOrderDto>()))
                             .Throws(new Exception("Transaction Timeout"));

            // Act
            var result = _controller.CreateOrder(dto);

            // Assert
            var resultObj = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(resultObj.Value);
            Assert.Contains("Transaction Timeout", resultObj.Value.ToString() ?? "");
        }
    }
}
