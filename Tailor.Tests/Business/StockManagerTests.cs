using Moq;
using Tailor.Business.Concrete;
using Tailor.DataAccess.Abstract;
using Tailor.DTO.DTOs.StockDtos;
using Tailor.Entity.Entities;
using Xunit;
using System;
using System.Collections.Generic;

namespace Tailor.Tests.Business
{
    public class StockManagerTests
    {
        private Mock<IStockDal> _stockDalMock;
        private Mock<IProductDal> _productDalMock;
        private Mock<IProductVariantDal> _productVariantDalMock;
        private Mock<IStockLogDal> _stockLogDalMock;
        private StockManager _stockManager;

        public StockManagerTests()
        {
            _stockDalMock = new Mock<IStockDal>();
            _productDalMock = new Mock<IProductDal>();
            _productVariantDalMock = new Mock<IProductVariantDal>();
            _stockLogDalMock = new Mock<IStockLogDal>();

            _stockManager = new StockManager(
                _stockDalMock.Object,
                _productDalMock.Object,
                _productVariantDalMock.Object,
                _stockLogDalMock.Object
            );
        }

        [Fact]
        [Trait("Scenario", "3")]
        public void TC24_AddStock_ShouldIncreaseQuantity_WhenPositiveValueProvided()
        {
            // Arrange
            var dto = new UpdateStockDto { ProductId = 1, VariantId = 10, Quantity = 5 };
            var variant = new ProductVariant { Id = 10, StockQuantity = 10 };
            _productVariantDalMock.Setup(s => s.GetById(10)).Returns(variant);

            // Act
            _stockManager.AddStock(dto);

            // Assert
            Assert.Equal(15, variant.StockQuantity);
            _productVariantDalMock.Verify(s => s.Update(It.Is<ProductVariant>(v => v.StockQuantity == 15)), Times.Once);
            _stockLogDalMock.Verify(s => s.Add(It.IsAny<StockLog>()), Times.Once);
        }

        [Fact]
        [Trait("Scenario", "3")]
        public void TC25_DecreaseStock_ShouldDecreaseQuantity_WhenValidValueProvided()
        {
            // Arrange
            int productId = 1;
            int variantId = 10;
            decimal decreaseAmount = 3;
            var variant = new ProductVariant { Id = variantId, StockQuantity = 10 };
            _productVariantDalMock.Setup(s => s.GetById(variantId)).Returns(variant);

            // Act
            _stockManager.DecreaseStock(productId, decreaseAmount, variantId);

            // Assert
            Assert.Equal(7, variant.StockQuantity);
            _productVariantDalMock.Verify(s => s.Update(It.Is<ProductVariant>(v => v.StockQuantity == 7)), Times.Once);
        }

        [Fact]
        [Trait("Scenario", "3")]
        public void TC26_DecreaseStock_ShouldThrowException_WhenStockIsInsufficient()
        {
            // Arrange
            int productId = 1;
            int variantId = 10;
            decimal decreaseAmount = 20; // More than available
            var variant = new ProductVariant { Id = variantId, StockQuantity = 10 };
            _productVariantDalMock.Setup(s => s.GetById(variantId)).Returns(variant);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _stockManager.DecreaseStock(productId, decreaseAmount, variantId));
            Assert.Contains("Yetersiz Stok", ex.Message);
        }

        [Fact]
        [Trait("Scenario", "3")]
        public void TC27_UpdateStock_ShouldOnlyAffectTargetVariant_AndNotOthers()
        {
            // Arrange
            var dto = new UpdateStockDto { ProductId = 1, VariantId = 10, Quantity = 5 };
            
            var variant10 = new ProductVariant { Id = 10, StockQuantity = 10 };
            var variant11 = new ProductVariant { Id = 11, StockQuantity = 20 };

            _productVariantDalMock.Setup(s => s.GetById(10)).Returns(variant10);
            _productVariantDalMock.Setup(s => s.GetById(11)).Returns(variant11);

            // Act
            _stockManager.AddStock(dto);

            // Assert
            Assert.Equal(15, variant10.StockQuantity);
            Assert.Equal(20, variant11.StockQuantity); // Should remain same
        }

        [Fact]
        [Trait("Scenario", "6")]
        public void TC51_StockAudit_ShouldLogAsSystem_WhenTriggeredBySale()
        {
            // Arrange
            int productId = 1, variantId = 10;
            decimal decreaseAmount = 5;
            var variant = new ProductVariant { Id = variantId, StockQuantity = 10 };
            _productVariantDalMock.Setup(s => s.GetById(variantId)).Returns(variant);

            // Act
            _stockManager.DecreaseStock(productId, decreaseAmount, variantId);

            // Assert
            _stockLogDalMock.Verify(s => s.Add(It.Is<StockLog>(l => 
                l.Source == "System" && 
                l.MovementType == "Satış" && 
                l.QuantityChange == -decreaseAmount)), Times.Once);
        }

        [Fact]
        [Trait("Scenario", "6")]
        public void TC52_StockAudit_ShouldLogAsAdmin_WhenTriggeredByManualUpdate()
        {
            // Arrange
            var dto = new UpdateStockDto { ProductId = 1, VariantId = 10, Quantity = 5 };
            var variant = new ProductVariant { Id = 10, StockQuantity = 10 };
            _productVariantDalMock.Setup(s => s.GetById(10)).Returns(variant);

            // Act
            _stockManager.AddStock(dto);

            // Assert
            _stockLogDalMock.Verify(s => s.Add(It.Is<StockLog>(l => 
                l.Source == "Admin" && 
                l.QuantityChange == 5)), Times.Once);
        }

        [Fact]
        [Trait("Scenario", "6")]
        public void TC53_StockAudit_ShouldCalculateNewStockQuantity_Correctly()
        {
            // Arrange
            var dto = new UpdateStockDto { ProductId = 1, VariantId = 10, Quantity = 5 };
            var variant = new ProductVariant { Id = 10, StockQuantity = 10 };
            _productVariantDalMock.Setup(s => s.GetById(10)).Returns(variant);

            // Act
            _stockManager.AddStock(dto);

            // Assert
            _stockLogDalMock.Verify(s => s.Add(It.Is<StockLog>(l => 
                l.NewStockLevel == 15)), Times.Once); // 10 + 5
        }
    }
}
