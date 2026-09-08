using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BookStore.IntegrationTests.Helpers;

namespace BookStore.IntegrationTests.Endpoints;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthEndpointsTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_RequestingAdmin_Returns201AsEditorWithToken()
    {
        // Arrange — a public client attempts to self-assign the Admin role.
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

        // Assert — public registration succeeds, but privilege escalation is prevented.
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("token", out var token));
        Assert.False(string.IsNullOrEmpty(token.GetString()));
        Assert.Equal("Editor", doc.RootElement.GetProperty("role").GetString());
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var client = _factory.CreateClient();
        var email = $"dup-{Guid.NewGuid():N}@test.com";
        var payload = new { email, password = "Pass123", name = "User", role = 1 };

        await client.PostAsJsonAsync("/auth/register", payload);
        var response = await client.PostAsJsonAsync("/auth/register", payload);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_MissingFields_Returns400WithErrors()
    {
        var client = _factory.CreateClient();
        var payload = new { email = "", password = "", name = "", role = 1 };

        var response = await client.PostAsJsonAsync("/auth/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("required", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400()
    {
        var client = _factory.CreateClient();
        var payload = new { email = "short@test.com", password = "12", name = "User", role = 1 };

        var response = await client.PostAsJsonAsync("/auth/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var client = _factory.CreateClient();
        var email = $"login-{Guid.NewGuid():N}@test.com";
        await client.PostAsJsonAsync("/auth/register", new { email, password = "Correct1", name = "Login", role = 1 });

        var response = await client.PostAsJsonAsync("/auth/login", new { email, password = "Correct1" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("token", out _));
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var client = _factory.CreateClient();
        var email = $"wrongpw-{Guid.NewGuid():N}@test.com";
        await client.PostAsJsonAsync("/auth/register", new { email, password = "RightPass", name = "User", role = 1 });

        var response = await client.PostAsJsonAsync("/auth/login", new { email, password = "WrongPass" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_NonExistentUser_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/login",
            new { email = "nobody@test.com", password = "AnyPass" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
