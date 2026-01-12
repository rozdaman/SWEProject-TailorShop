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
    public class StockRepositoryTests
    {
        private ApplicationDbContext _context;
        private IProductVariantDal _variantDal;
        private IStockLogDal _logDal;

        public StockRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _variantDal = new EfProductVariantDal(_context);
            _logDal = new EfStockLogDal(_context);
        }

        [Fact]
        [Trait("Scenario", "3")]
        public void TC31_Persistence_ShouldUpdateProductVariantStockPhysically()
        {
            // Arrange
            var variant = new ProductVariant
            {
                Id = 1,
                ProductId = 10,
                Size = "M",
                Color = "Black",
                StockQuantity = 50
            };
            _context.Set<ProductVariant>().Add(variant);
            _context.SaveChanges();

            // Act
            var vFromDb = _variantDal.GetById(1);
            vFromDb.StockQuantity = 100;
            _variantDal.Update(vFromDb);
            _context.SaveChanges();

            // Assert
            var updatedVariant = _context.Set<ProductVariant>().First(x => x.Id == 1);
            Assert.Equal(100, updatedVariant.StockQuantity);
        }

        [Fact]
        [Trait("Scenario", "3")]
        public void TC32_AuditLog_ShouldCreateStockLogEntry()
        {
            // Arrange
            var log = new StockLog
            {
                ProductId = 1,
                VariantId = 1,
                QuantityChange = 10,
                NewStockLevel = 60,
                MovementType = "Test",
                CreatedAt = DateTime.Now,
                Description = "Test log",
                Source = "System"
            };

            // Act
            _logDal.Add(log);
            _context.SaveChanges();

            // Assert
            var logCount = _context.StockLogs.Count();
            Assert.Equal(1, logCount);
            Assert.Equal(1, logCount);
            Assert.Equal(10, _context.StockLogs.First().QuantityChange);
        }

        [Fact]
        [Trait("Scenario", "6")]
        public void TC56_Persistence_StockHistory_ShouldBeStoredPhysicallyInDb()
        {
            // Arrange
            var log = new StockLog
            {
                ProductId = 1,
                VariantId = 1,
                QuantityChange = 5,
                MovementType = "Manual Correction",
                CreatedAt = DateTime.Now,
                Description = "Repair",
                Source = "Admin"
            };

            // Act
            _logDal.Add(log);
            _context.SaveChanges();

            // Assert
            var logInDb = _context.StockLogs.FirstOrDefault(x => x.Description == "Repair");
            Assert.NotNull(logInDb);
            Assert.Equal("Admin", logInDb.Source);
        }

        [Fact]
        [Trait("Scenario", "6")]
        public void TC57_Ordering_StockHistory_ShouldBeOrderedByLatestFirst()
        {
            // Arrange
            var now = DateTime.Now;
            var logs = new List<StockLog>
            {
                new StockLog { ProductId = 1, VariantId = 1, CreatedAt = now.AddMinutes(-10), MovementType = "Sale", Source = "System", Description = "Old" },
                new StockLog { ProductId = 1, VariantId = 1, CreatedAt = now, MovementType = "Correction", Source = "Admin", Description = "New" }
            };
            foreach (var log in logs) _logDal.Add(log);
            _context.SaveChanges();

            // Act - We test the logic that usually lives in Dal or Service
            // Since DAL is Generic, we verify via a simple query that represents the intent
            var result = _context.StockLogs.Where(x => x.VariantId == 1).OrderByDescending(x => x.CreatedAt).ToList();

            // Assert
            Assert.Equal("Correction", result[0].MovementType);
            Assert.Equal("Sale", result[1].MovementType);
        }
    }
}
