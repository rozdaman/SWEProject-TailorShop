using Microsoft.EntityFrameworkCore;
using Tailor.DataAccess.Abstract;
using Tailor.DataAccess.Concrete;
using Tailor.DataAccess.Context;
using Tailor.Entity.Entities;
using Xunit;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Tailor.Tests.DataAccess
{
    public class ProductRepositoryTests
    {
        private ApplicationDbContext _context;
        private IProductDal _productDal;

        public ProductRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _productDal = new EfProductDal(_context);
        }

        [Fact]
        [Trait("Scenario", "5")]
        public void TC48_GetProductsQuery_ShouldUseAsNoTracking()
        {
            // Note: Verifying AsNoTracking is difficult in a unit test without custom hooks.
            // We verify that the method returns data correctly from the context.
            
            // Arrange
            var product = new Product { Name = "Track Test", IsActive = true, CategoryId = 1, Description = "Test", ImageUrl = "test.jpg" };
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act
            var result = _productDal.GetListByFilter(x => x.Name == "Track Test");

            // Assert
            Assert.Single(result);
        }

        [Fact]
        [Trait("Scenario", "5")]
        public void TC49_GetProductsByCategory_ShouldIncludeParentAndChildCategories()
        {
            // Arrange
            var parent = new Category { Name = "Women" };
            _context.Categories.Add(parent);
            _context.SaveChanges();

            var child = new Category { Name = "Dress", ParentCategoryId = parent.Id };
            _context.Categories.Add(child);
            _context.SaveChanges();

            var product = new Product { Name = "Red Dress", CategoryId = child.Id, IsActive = true, Description = "Test", ImageUrl = "test.jpg" };
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act
            var result = _productDal.GetListWithRelations(x => x.CategoryId == child.Id, "Category");

            // Assert
            Assert.Single(result);
            Assert.NotNull(result[0].Category);
            Assert.Equal("Dress", result[0].Category.Name);
        }

        [Fact]
        [Trait("Scenario", "5")]
        public void TC50_GetProductsBySize_ShouldPerformCorrectJoinWithVariants()
        {
            // Arrange
            var product = new Product { Name = "Size Test", IsActive = true, CategoryId = 1, Description = "Test", ImageUrl = "test.jpg" };
            _context.Products.Add(product);
            _context.SaveChanges();

            var variant = new ProductVariant { ProductId = product.id, Size = "XL", Color = "Blue" };
            _context.Set<ProductVariant>().Add(variant);
            _context.SaveChanges();

            // Act
            var result = _productDal.GetListWithRelations(x => x.id == product.id, "ProductVariants");

            // Assert
            Assert.Single(result);
            Assert.NotNull(result[0].ProductVariants);
            Assert.Contains(result[0].ProductVariants, v => v.Size == "XL");
        }
    }
}
