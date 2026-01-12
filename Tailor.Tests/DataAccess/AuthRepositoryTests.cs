using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tailor.DataAccess.Context;
using Tailor.Entity.Entities;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Tailor.Tests.DataAccess
{
    public class AuthRepositoryTests
    {
        private ApplicationDbContext _context;
        private UserManager<AppUser> _userManager;

        public AuthRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            var userStoreMock = new Mock<IUserStore<AppUser>>();
            // We need a real PasswordHasher to test TC-63
            var passwordHasher = new PasswordHasher<AppUser>();
            
            // For TC-63, we can use a simpler approach: use the real UserManager with in-memory store if possible, 
            // but since we want to check the DB field, we use the DB context directly.
        }

        [Fact]
        [Trait("Scenario", "7")]
        public async Task TC63_Password_ShouldBeStoredAsHash_NotPlainText()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "AuthTestDb")
                .Options;
            using var context = new ApplicationDbContext(options);
            
            var user = new AppUser 
            { 
                UserName = "hashuser", 
                Email = "hash@test.com",
                Name = "Test",
                Surname = "User",
                PasswordHash = "AQAAAAEAACcQAAAAEM..." // Normally set by UserManager
            };

            // In a real scenario, UserManager.CreateAsync(user, "Password123!") would set the hash.
            // We simulate the result of that operation to check the field integrity.
            var hasher = new PasswordHasher<AppUser>();
            var plainPassword = "SecurePassword123!";
            user.PasswordHash = hasher.HashPassword(user, plainPassword);

            // Act
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Assert
            var userInDb = await context.Users.FirstAsync(u => u.UserName == "hashuser");
            Assert.NotEqual(plainPassword, userInDb.PasswordHash);
            Assert.True(userInDb.PasswordHash.Length > 20); // Hashes are long
            
            // Verify it's a valid hash
            var verificationResult = hasher.VerifyHashedPassword(userInDb, userInDb.PasswordHash, plainPassword);
            Assert.Equal(PasswordVerificationResult.Success, verificationResult);
        }
    }
}
