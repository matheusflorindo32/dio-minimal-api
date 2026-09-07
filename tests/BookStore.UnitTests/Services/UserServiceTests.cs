using BookStore.Api.Domain.DTOs;
using BookStore.Api.Domain.Enums;
using BookStore.Api.Domain.Services;
using BookStore.UnitTests.Helpers;

namespace BookStore.UnitTests.Services;

public class UserServiceTests
{
    [Fact]
    public void Create_ValidRequest_CreatesUserWithHashedPassword()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new UserService(context);
        var request = new CreateUserRequest("test@test.com", "Secure123", "Test User", UserRole.Editor);

        // Act
        var user = service.Create(request);

        // Assert
        Assert.NotEqual(0, user.Id);
        Assert.Equal("test@test.com", user.Email);
        Assert.Equal("Test User", user.Name);
        Assert.Equal("Editor", user.Role);
        // Password must be hashed, never stored as plain text
        Assert.NotEqual("Secure123", user.PasswordHash);
        Assert.Contains(".", user.PasswordHash); // salt.hash format
    }

    [Fact]
    public void Login_CorrectCredentials_ReturnsUser()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new UserService(context);
        service.Create(new CreateUserRequest("login@test.com", "MyPass99", "Login User", UserRole.Admin));

        // Act
        var result = service.Login(new LoginRequest("login@test.com", "MyPass99"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("login@test.com", result.Email);
    }

    [Fact]
    public void Login_WrongPassword_ReturnsNull()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new UserService(context);
        service.Create(new CreateUserRequest("wrong@test.com", "CorrectPass", "User", UserRole.Editor));

        // Act
        var result = service.Login(new LoginRequest("wrong@test.com", "WrongPass"));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Login_NonExistentEmail_ReturnsNull()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new UserService(context);

        // Act
        var result = service.Login(new LoginRequest("noone@test.com", "AnyPass"));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void EmailExists_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new UserService(context);
        service.Create(new CreateUserRequest("exists@test.com", "Pass123", "User", UserRole.Editor));

        // Act & Assert
        Assert.True(service.EmailExists("exists@test.com"));
    }

    [Fact]
    public void EmailExists_NonExistentEmail_ReturnsFalse()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new UserService(context);

        // Act & Assert
        Assert.False(service.EmailExists("ghost@test.com"));
    }

    [Fact]
    public void GetById_ExistingId_ReturnsUser()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new UserService(context);
        var created = service.Create(new CreateUserRequest("find@test.com", "Pass123", "Find Me", UserRole.Admin));

        // Act
        var result = service.GetById(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("find@test.com", result.Email);
    }

    [Fact]
    public void GetById_NonExistentId_ReturnsNull()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new UserService(context);

        // Act
        var result = service.GetById(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetAll_ReturnsPagedResults()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new UserService(context);
        for (int i = 1; i <= 15; i++)
            service.Create(new CreateUserRequest($"user{i}@test.com", "Pass123", $"User {i}", UserRole.Editor));

        // Act — page 1 should have up to 10 (PageSize), page 2 should have 5
        var page1 = service.GetAll(1);
        var page2 = service.GetAll(2);

        // Assert
        // Note: seed data may add 1 admin user, so counts may include it
        Assert.True(page1.Count <= 10);
        Assert.True(page2.Count > 0);
    }
}
