using Moq;
using Microsoft.AspNetCore.Mvc;
using Tailor.API.Controllers;
using Tailor.Business.Abstract;
using Tailor.DTO.DTOs.ProductDtos;
using Xunit;
using System.Collections.Generic;
using System.Diagnostics;
using AutoMapper;

namespace Tailor.Tests.API
{
    public class ProductsControllerTests
    {
        private Mock<IProductService> _productServiceMock;
        private Mock<IMapper> _mapperMock;
        private ProductsController _controller;

        public ProductsControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _mapperMock = new Mock<IMapper>();
            _controller = new ProductsController(_productServiceMock.Object, _mapperMock.Object);
        }

        [Fact]
        [Trait("Scenario", "5")]
        public void TC45_FilterProducts_ShouldRespondUnder200ms()
        {
            // Arrange
            _productServiceMock.Setup(s => s.GetFilteredProducts(It.IsAny<int?>(), It.IsAny<string?>()))
                               .Returns(new List<ResultProductListDto>());

            // Act
            var sw = Stopwatch.StartNew();
            var result = _controller.GetFiltered(5, "L");
            sw.Stop();

            // Assert
            Assert.True(sw.ElapsedMilliseconds < 200, $"Execution took {sw.ElapsedMilliseconds}ms, which exceeds the 200ms target.");
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        [Trait("Scenario", "5")]
        public void TC46_FilterProducts_ShouldReturnOk_WithData()
        {
            // Arrange
            var products = new List<ResultProductListDto> { new ResultProductListDto { Id = 1 } };
            _productServiceMock.Setup(s => s.GetFilteredProducts(5, "M")).Returns(products);

            // Act
            var result = _controller.GetFiltered(5, "M");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedValue = Assert.IsType<List<ResultProductListDto>>(okResult.Value);
            Assert.Single(returnedValue);
        }

        [Fact]
        [Trait("Scenario", "5")]
        public void TC47_FilterProducts_ShouldMapToProductDto_Correctly()
        {
            // Arrange
            var products = new List<ResultProductListDto> 
            { 
                new ResultProductListDto { Id = 1, Name = "Test Product", Price = 100 } 
            };
            _productServiceMock.Setup(s => s.GetFilteredProducts(It.IsAny<int?>(), It.IsAny<string?>())).Returns(products);

            // Act
            var result = _controller.GetFiltered(null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedValue = Assert.IsType<List<ResultProductListDto>>(okResult.Value);
            Assert.Equal("Test Product", returnedValue[0].Name);
        }
    }
}
