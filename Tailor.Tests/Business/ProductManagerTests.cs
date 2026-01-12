using Moq;
using Tailor.Business.Concrete;
using Tailor.DataAccess.Abstract;
using Tailor.DTO.DTOs.ProductDtos;
using Tailor.Entity.Entities;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;

namespace Tailor.Tests.Business
{
    public class ProductManagerTests
    {
        private Mock<IProductDal> _productDalMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IProductDisplayDal> _productDisplayDalMock;
        private Mock<IProductVariantDal> _productVariantDalMock;
        private Mock<IStockLogDal> _stockLogDalMock;
        private ProductManager _productManager;

        public ProductManagerTests()
        {
            _productDalMock = new Mock<IProductDal>();
            _mapperMock = new Mock<IMapper>();
            _productDisplayDalMock = new Mock<IProductDisplayDal>();
            _productVariantDalMock = new Mock<IProductVariantDal>();
            _stockLogDalMock = new Mock<IStockLogDal>();

            _productManager = new ProductManager(
                _productDalMock.Object,
                _mapperMock.Object,
                _productDisplayDalMock.Object,
                _productVariantDalMock.Object,
                _stockLogDalMock.Object
            );
        }

        [Fact]
        [Trait("Scenario", "5")]
        public void TC41_GetFilteredProducts_ShouldReturnSubCategoryProducts_WhenHierarchySelected()
        {
            // Arrange
            int categoryId = 5; // e.g., Dress
            var products = new List<Product>
            {
                new Product { id = 1, Name = "Summer Dress", CategoryId = 5, IsActive = true },
                new Product { id = 2, Name = "Evening Dress", CategoryId = 5, IsActive = true },
                new Product { id = 3, Name = "Jeans", CategoryId = 10, IsActive = true }
            };

            _productDalMock.Setup(d => d.GetListWithRelations(
                It.IsAny<Expression<Func<Product, bool>>>(), 
                It.IsAny<string[]>()))
                .Returns((Expression<Func<Product, bool>> filter, string[] includes) => 
                    products.Where(filter.Compile()).ToList());

            _mapperMock.Setup(m => m.Map<List<ResultProductListDto>>(It.IsAny<List<Product>>()))
                       .Returns((List<Product> src) => src.Select(p => new ResultProductListDto { Id = p.id }).ToList());

            // Act
            var result = _productManager.GetFilteredProducts(categoryId, null);

            // Assert
            Assert.Equal(2, result.Count);
            _productDalMock.Verify(d => d.GetListWithRelations(It.IsAny<Expression<Func<Product, bool>>>(), "Category", "ProductVariants"), Times.Once);
        }

        [Fact]
        [Trait("Scenario", "5")]
        public void TC42_GetFilteredProducts_ShouldApplySizeFilter_Correctly()
        {
            // Arrange
            string size = "L";
            var products = new List<Product>
            {
                new Product 
                { 
                    id = 1, IsActive = true,
                    ProductVariants = new List<ProductVariant> { new ProductVariant { Size = "L" } } 
                },
                new Product 
                { 
                    id = 2, IsActive = true,
                    ProductVariants = new List<ProductVariant> { new ProductVariant { Size = "S" } } 
                }
            };

            _productDalMock.Setup(d => d.GetListWithRelations(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
                .Returns((Expression<Func<Product, bool>> filter, string[] includes) => 
                    products.Where(filter.Compile()).ToList());

            _mapperMock.Setup(m => m.Map<List<ResultProductListDto>>(It.IsAny<List<Product>>()))
                       .Returns((List<Product> src) => src.Select(p => new ResultProductListDto { Id = p.id }).ToList());

            // Act
            var result = _productManager.GetFilteredProducts(null, size);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        [Trait("Scenario", "5")]
        public void TC43_GetFilteredProducts_ShouldReturnEmptyList_WhenNoMatchExists()
        {
            // Arrange
            _productDalMock.Setup(d => d.GetListWithRelations(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
                .Returns(new List<Product>());

            _mapperMock.Setup(m => m.Map<List<ResultProductListDto>>(It.IsAny<List<Product>>()))
                       .Returns(new List<ResultProductListDto>());

            // Act
            var result = _productManager.GetFilteredProducts(999, "XXL");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        [Trait("Scenario", "5")]
        public void TC44_GetFilteredProducts_ShouldCombineMultipleFilters_Properly()
        {
            // Arrange
            int categoryId = 5;
            string size = "M";
            var products = new List<Product>
            {
                new Product 
                { 
                    id = 1, CategoryId = 5, IsActive = true,
                    ProductVariants = new List<ProductVariant> { new ProductVariant { Size = "M" } } 
                },
                new Product 
                { 
                    id = 2, CategoryId = 5, IsActive = true,
                    ProductVariants = new List<ProductVariant> { new ProductVariant { Size = "L" } } 
                },
                new Product 
                { 
                    id = 3, CategoryId = 10, IsActive = true,
                    ProductVariants = new List<ProductVariant> { new ProductVariant { Size = "M" } } 
                }
            };

            _productDalMock.Setup(d => d.GetListWithRelations(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
                .Returns((Expression<Func<Product, bool>> filter, string[] includes) => 
                    products.Where(filter.Compile()).ToList());

            _mapperMock.Setup(m => m.Map<List<ResultProductListDto>>(It.IsAny<List<Product>>()))
                       .Returns((List<Product> src) => src.Select(p => new ResultProductListDto { Id = p.id }).ToList());

            // Act
            var result = _productManager.GetFilteredProducts(categoryId, size);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }
    }
}
