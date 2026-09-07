using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BookStore.IntegrationTests.Helpers;

namespace BookStore.IntegrationTests.Endpoints;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthEndpointsTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_ValidData_Returns201WithToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var payload = new
        {
            email = $"new-{Guid.NewGuid():N}@test.com",
            password = "Pass123",
            name = "New User",
            role = 0
        };

        // Act
        var response = await client.PostAsJsonAsync("/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("token", out var token));
        Assert.False(string.IsNullOrEmpty(token.GetString()));
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = $"dup-{Guid.NewGuid():N}@test.com";
        var payload = new { email, password = "Pass123", name = "User", role = 0 };

        await client.PostAsJsonAsync("/auth/register", payload);

        // Act — register again with same email
        var response = await client.PostAsJsonAsync("/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_MissingFields_Returns400WithErrors()
    {
        // Arrange
        var client = _factory.CreateClient();
        var payload = new { email = "", password = "", name = "", role = 0 };

        // Act
        var response = await client.PostAsJsonAsync("/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("required", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400()
    {
        // Arrange
        var client = _factory.CreateClient();
        var payload = new { email = "short@test.com", password = "12", name = "User", role = 0 };

        // Act
        var response = await client.PostAsJsonAsync("/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = $"login-{Guid.NewGuid():N}@test.com";
        await client.PostAsJsonAsync("/auth/register", new { email, password = "Correct1", name = "Login", role = 0 });

        // Act
        var response = await client.PostAsJsonAsync("/auth/login", new { email, password = "Correct1" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("token", out _));
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = $"wrongpw-{Guid.NewGuid():N}@test.com";
        await client.PostAsJsonAsync("/auth/register", new { email, password = "RightPass", name = "User", role = 0 });

        // Act
        var response = await client.PostAsJsonAsync("/auth/login", new { email, password = "WrongPass" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_NonExistentUser_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/auth/login",
            new { email = "nobody@test.com", password = "AnyPass" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
