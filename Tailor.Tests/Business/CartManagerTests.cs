using Moq;
using Tailor.Business.Concrete;
using Tailor.DataAccess.Abstract;
using Tailor.DTO.DTOs.CartDtos;
using Tailor.Entity.Entities;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Tailor.Business.Abstract;

namespace Tailor.Tests.Business
{
    public class CartManagerTests
    {
        private Mock<ICartDal> _cartDalMock;
        private Mock<IProductDal> _productDalMock;
        private Mock<IStockService> _stockServiceMock;
        private CartManager _cartManager;

        public CartManagerTests()
        {
            _cartDalMock = new Mock<ICartDal>();
            _productDalMock = new Mock<IProductDal>();
            _stockServiceMock = new Mock<IStockService>();

            _cartManager = new CartManager(
                _cartDalMock.Object,
                _productDalMock.Object,
                _stockServiceMock.Object
            );
        }

        [Fact]
        [Trait("Scenario", "4")]
        public void TC33_MergeCarts_ShouldLinkGuestItemsToUser_WhenUserCartIsEmpty()
        {
            // Arrange
            string sessionId = "guest-session";
            int userId = 1;

            var guestCart = new ShoppingCart
            {
                SessionId = sessionId,
                ShoppingCartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem { ProductId = 10, Quantity = 2, VariantId = 1 }
                }
            };

            _cartDalMock.Setup(d => d.GetCartWithItems(null, sessionId)).Returns(guestCart);
            _cartDalMock.Setup(d => d.GetCartWithItems(userId, null)).Returns((ShoppingCart)null);

            // Act
            _cartManager.MergeCarts(sessionId, userId);

            // Assert
            Assert.Equal(userId, guestCart.UserId);
            Assert.Equal(string.Empty, guestCart.SessionId);
            _cartDalMock.Verify(d => d.Update(guestCart), Times.Once);
        }

        [Fact]
        [Trait("Scenario", "4")]
        public void TC34_MergeCarts_ShouldUpdateQuantity_WhenItemAlreadyExistsInUserCart()
        {
            // Arrange
            string sessionId = "guest-session";
            int userId = 1;

            var guestCart = new ShoppingCart
            {
                SessionId = sessionId,
                ShoppingCartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem { ProductId = 10, Quantity = 2, VariantId = 1 }
                }
            };

            var userCart = new ShoppingCart
            {
                UserId = userId,
                ShoppingCartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem { ProductId = 10, Quantity = 3, VariantId = 1 }
                }
            };

            _cartDalMock.Setup(d => d.GetCartWithItems(null, sessionId)).Returns(guestCart);
            _cartDalMock.Setup(d => d.GetCartWithItems(userId, null)).Returns(userCart);

            // Act
            _cartManager.MergeCarts(sessionId, userId);

            // Assert
            var mergedItem = userCart.ShoppingCartItems.First(x => x.ProductId == 10 && x.VariantId == 1);
            Assert.Equal(5, mergedItem.Quantity);
            _cartDalMock.Verify(d => d.Update(userCart), Times.Once);
            _cartDalMock.Verify(d => d.Delete(guestCart), Times.Once);
        }

        [Fact]
        [Trait("Scenario", "4")]
        public void TC35_MergeCarts_ShouldDeleteGuestCart_AfterSuccessfulMerge()
        {
            // Arrange
            string sessionId = "guest-session";
            int userId = 1;

            var guestCart = new ShoppingCart { ShoppingCartItems = new List<ShoppingCartItem> { new ShoppingCartItem() } };
            var userCart = new ShoppingCart { ShoppingCartItems = new List<ShoppingCartItem> { new ShoppingCartItem() } };

            _cartDalMock.Setup(d => d.GetCartWithItems(null, sessionId)).Returns(guestCart);
            _cartDalMock.Setup(d => d.GetCartWithItems(userId, null)).Returns(userCart);

            // Act
            _cartManager.MergeCarts(sessionId, userId);

            // Assert
            _cartDalMock.Verify(d => d.Delete(guestCart), Times.Once);
        }

        [Fact]
        [Trait("Scenario", "4")]
        public void TC36_MergeCarts_ShouldDoNothing_WhenGuestCartIsNull()
        {
            // Arrange
            string sessionId = "empty-session";
            int userId = 1;

            _cartDalMock.Setup(d => d.GetCartWithItems(null, sessionId)).Returns((ShoppingCart)null);

            // Act
            _cartManager.MergeCarts(sessionId, userId);

            // Assert
            _cartDalMock.Verify(d => d.Update(It.IsAny<ShoppingCart>()), Times.Never);
            _cartDalMock.Verify(d => d.Delete(It.IsAny<ShoppingCart>()), Times.Never);
        }
    }
}
