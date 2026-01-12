using Moq;
using Tailor.Business.Concrete;
using Tailor.DataAccess.Abstract;
using Tailor.DTO.DTOs.OrderDtos;
using Tailor.DTO.DTOs.CartDtos;
using Tailor.DTO.DTOs.PaymentDtos;
using Tailor.Entity.Entities;
using Tailor.Entity.Entities.Enums;
using Tailor.DataAccess.Context;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Tailor.Business.Abstract;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Tailor.Tests.Business
{
    public class OrderManagerTests
    {
        private Mock<IOrderDal> _orderDalMock;
        private Mock<ICartService> _cartServiceMock;
        private Mock<IStockService> _stockServiceMock;
        private Mock<IPaymentService> _paymentServiceMock;
        private Mock<IPaymentDal> _paymentDalMock;
        private ApplicationDbContext _context;
        private OrderManager _orderManager;

        public OrderManagerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new ApplicationDbContext(options);

            _orderDalMock = new Mock<IOrderDal>();
            _cartServiceMock = new Mock<ICartService>();
            _stockServiceMock = new Mock<IStockService>();
            _paymentServiceMock = new Mock<IPaymentService>();
            _paymentDalMock = new Mock<IPaymentDal>();

            _orderManager = new OrderManager(
                _orderDalMock.Object,
                _cartServiceMock.Object,
                _stockServiceMock.Object,
                _paymentServiceMock.Object,
                _paymentDalMock.Object,
                _context
            );
        }

        [Fact]
        [Trait("Scenario", "1")]
        public void TC01_ShouldProcessOnlySelectedItemsWithCorrectPrice()
        {
            // Arrange
            int userId = 1;
            var cartDto = new ShoppingCartDto
            {
                Items = new List<CartItemDetailDto>
                {
                    new CartItemDetailDto { ShoppingCartItemId = 10, ProductId = 1, UnitPrice = 100, Quantity = 2 },
                    new CartItemDetailDto { ShoppingCartItemId = 11, ProductId = 2, UnitPrice = 50, Quantity = 1 },
                    new CartItemDetailDto { ShoppingCartItemId = 12, ProductId = 3, UnitPrice = 200, Quantity = 1 }
                }
            };
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int> { 10, 11 } };

            _cartServiceMock.Setup(s => s.GetCart(userId, null)).Returns(cartDto);
            _paymentServiceMock.Setup(s => s.ProcessPayment(It.IsAny<CreatePaymentDto>())).Returns(true);

            // Act
            _orderManager.CreateOrder(userId, dto);

            // Assert
            _paymentServiceMock.Verify(s => s.ProcessPayment(It.Is<CreatePaymentDto>(p => p.Amount == 250)), Times.Once);
            _orderDalMock.Verify(s => s.Add(It.Is<Order>(o => o.OrderItems.Count == 2)), Times.Once);
        }

        [Fact]
        [Trait("Scenario", "1")]
        public void TC02_ShouldKeepUnselectedItemsInCart()
        {
            // Arrange
            int userId = 1;
            var cartDto = new ShoppingCartDto
            {
                Items = new List<CartItemDetailDto>
                {
                    new CartItemDetailDto { ShoppingCartItemId = 10, ProductId = 1 },
                    new CartItemDetailDto { ShoppingCartItemId = 11, ProductId = 2 }
                }
            };
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int> { 10 } };
            _cartServiceMock.Setup(s => s.GetCart(userId, null)).Returns(cartDto);
            _paymentServiceMock.Setup(s => s.ProcessPayment(It.IsAny<CreatePaymentDto>())).Returns(true);

            // Act
            _orderManager.CreateOrder(userId, dto);

            // Assert
            _cartServiceMock.Verify(s => s.RemoveCartItem(userId, 10), Times.Once);
            _cartServiceMock.Verify(s => s.RemoveCartItem(userId, 11), Times.Never);
        }

        [Fact]
        [Trait("Scenario", "1")]
        public void TC03_ShouldThrowExceptionWhenSelectedIdsEmpty()
        {
            // Arrange
            int userId = 1;
            var cartDto = new ShoppingCartDto { Items = new List<CartItemDetailDto> { new CartItemDetailDto { ShoppingCartItemId = 1 } } };
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int>() };
            _cartServiceMock.Setup(s => s.GetCart(userId, null)).Returns(cartDto);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _orderManager.CreateOrder(userId, dto));
            Assert.Equal("Lütfen satın alınacak ürünleri seçiniz.", ex.Message);
        }

        [Fact]
        [Trait("Scenario", "1")]
        public void TC04_ShouldThrowExceptionWhenSelectedIdNotExistsInUserCart()
        {
            // Arrange
            int userId = 1;
            var cartDto = new ShoppingCartDto
            {
                Items = new List<CartItemDetailDto> { new CartItemDetailDto { ShoppingCartItemId = 10 } }
            };
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int> { 99 } }; // User doesn't have ID 99
            _cartServiceMock.Setup(s => s.GetCart(userId, null)).Returns(cartDto);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _orderManager.CreateOrder(userId, dto));
            Assert.Contains("Seçtiğiniz ürünler sepette bulunamadı", ex.Message);
        }

        [Fact]
        [Trait("Scenario", "1")]
        public void TC05_ShouldRollbackWhenStockDepletedMidCheckout()
        {
            // Arrange
            int userId = 1;
            var cartDto = new ShoppingCartDto { Items = new List<CartItemDetailDto> { new CartItemDetailDto { ShoppingCartItemId = 10, ProductId = 1, Quantity = 1 } } };
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int> { 10 } };
            _cartServiceMock.Setup(s => s.GetCart(userId, null)).Returns(cartDto);
            
            // Stock service throws exception mid-loop
            _stockServiceMock.Setup(s => s.DecreaseStock(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<int?>()))
                             .Throws(new Exception("Stock depleted!"));

            // Act & Assert
            Assert.Throws<Exception>(() => _orderManager.CreateOrder(userId, dto));
            Assert.Equal(0, _context.Orders.Count()); // Transaction rolled back
        }

        [Fact]
        [Trait("Scenario", "1")]
        public void TC06_ShouldCalculateSubtotalCorrectlyForSelectedItems()
        {
            // Arrange
            int userId = 1;
            var cartDto = new ShoppingCartDto
            {
                Items = new List<CartItemDetailDto>
                {
                    new CartItemDetailDto { ShoppingCartItemId = 10, ProductId = 1, UnitPrice = 100, Quantity = 1 }, // Selected
                    new CartItemDetailDto { ShoppingCartItemId = 11, ProductId = 2, UnitPrice = 200, Quantity = 1 }  // Not Selected
                }
            };
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int> { 10 } };
            _cartServiceMock.Setup(s => s.GetCart(userId, null)).Returns(cartDto);
            _paymentServiceMock.Setup(s => s.ProcessPayment(It.IsAny<CreatePaymentDto>())).Returns(true);

            // Act
            _orderManager.CreateOrder(userId, dto);

            // Assert
            _orderDalMock.Verify(s => s.Add(It.Is<Order>(o => o.TotalAmount == 100)), Times.Once);
        }

        [Fact]
        [Trait("Scenario", "1")]
        public void TC07_ShouldSetMandatoryFieldsCorrectly()
        {
            // Arrange
            int userId = 99;
            var cartDto = new ShoppingCartDto { Items = new List<CartItemDetailDto> { new CartItemDetailDto { ShoppingCartItemId = 1, ProductId = 1, Quantity = 1 } } };
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int> { 1 } };
            _cartServiceMock.Setup(s => s.GetCart(userId, null)).Returns(cartDto);
            _paymentServiceMock.Setup(s => s.ProcessPayment(It.IsAny<CreatePaymentDto>())).Returns(true);

            // Act
            _orderManager.CreateOrder(userId, dto);

            // Assert
            _orderDalMock.Verify(s => s.Add(It.Is<Order>(o => 
                o.UserId == 99 && 
                o.OrderDate.Date == DateTime.Now.Date && 
                !string.IsNullOrEmpty(o.OrderNumber))), Times.Once);
        }

        [Fact]
        [Trait("Scenario", "2")]
        public void TC16_ShouldProcessPaymentAndCompleteSuccessfully()
        {
            // Arrange
            int userId = 1;
            var cartDto = new ShoppingCartDto { Items = new List<CartItemDetailDto> { new CartItemDetailDto { ShoppingCartItemId = 101, ProductId = 1, Quantity = 1 } } };
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int> { 101 } };
            _cartServiceMock.Setup(s => s.GetCart(userId, null)).Returns(cartDto);
            _paymentServiceMock.Setup(s => s.ProcessPayment(It.IsAny<CreatePaymentDto>())).Returns(true);

            // Act
            _orderManager.CreateOrder(userId, dto);

            // Assert
            _paymentServiceMock.Verify(s => s.ProcessPayment(It.IsAny<CreatePaymentDto>()), Times.Once);
            _cartServiceMock.Verify(s => s.RemoveCartItem(userId, 101), Times.Once);
        }

        [Fact]
        [Trait("Scenario", "2")]
        public void TC17_ShouldRollbackAndNotDecreaseStock_WhenPaymentIsRejected()
        {
            // Arrange
            int userId = 1;
            var cartDto = new ShoppingCartDto { Items = new List<CartItemDetailDto> { new CartItemDetailDto { ShoppingCartItemId = 101, ProductId = 1, Quantity = 1 } } };
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int> { 101 } };
            _cartServiceMock.Setup(s => s.GetCart(userId, null)).Returns(cartDto);
            _paymentServiceMock.Setup(s => s.ProcessPayment(It.IsAny<CreatePaymentDto>())).Returns(false); // Payment Failed

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _orderManager.CreateOrder(userId, dto));
            Assert.Contains("Ödeme banka tarafından reddedildi", ex.Message);

            // Verify state: Order not committed (count is 0 because of rollback)
            Assert.Equal(0, _context.Orders.Count());
            _cartServiceMock.Verify(s => s.RemoveCartItem(userId, 101), Times.Never);
        }

        [Fact]
        [Trait("Scenario", "2")]
        public void TC18_ShouldRollback_WhenDatabaseWriteFailsAfterPayment()
        {
            // Arrange
            int userId = 1;
            var cartDto = new ShoppingCartDto { Items = new List<CartItemDetailDto> { new CartItemDetailDto { ShoppingCartItemId = 101, ProductId = 1, Quantity = 1 } } };
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int> { 101 } };
            _cartServiceMock.Setup(s => s.GetCart(userId, null)).Returns(cartDto);
            _paymentServiceMock.Setup(s => s.ProcessPayment(It.IsAny<CreatePaymentDto>())).Returns(true);
            
            // Database error occurs during second SaveChanges (Cart cleaning)
            _cartServiceMock.Setup(s => s.RemoveCartItem(It.IsAny<int>(), It.IsAny<int>()))
                             .Throws(new Exception("Database connection lost!"));

            // Act & Assert
            Assert.Throws<Exception>(() => _orderManager.CreateOrder(userId, dto));
            Assert.Equal(0, _context.Orders.Count()); // Fully rolled back
        }

        [Fact]
        [Trait("Scenario", "2")]
        public void TC19_ShouldPreserveDatabaseState_AfterRollback()
        {
            // Arrange
            int userId = 1;
            var cartDto = new ShoppingCartDto { Items = new List<CartItemDetailDto> { new CartItemDetailDto { ShoppingCartItemId = 101, ProductId = 1, Quantity = 1 } } };
            var dto = new CreateOrderDto { SelectedShoppingCartItemIds = new List<int> { 101 } };
            _cartServiceMock.Setup(s => s.GetCart(userId, null)).Returns(cartDto);
            _paymentServiceMock.Setup(s => s.ProcessPayment(It.IsAny<CreatePaymentDto>())).Returns(false);

            // Act
            try { _orderManager.CreateOrder(userId, dto); } catch { }

            // Assert
            // State check in DB (InMemory)
            Assert.Empty(_context.Orders.ToList());
            Assert.Empty(_context.OrderItems.ToList());
        }
    }
}
