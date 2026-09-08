using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookStore.IntegrationTests.Helpers;

/// <summary>
/// Helper to authenticate test clients with the roles supported by the API.
/// Public registration always creates Editor users; Admin access uses the seeded development account.
/// </summary>
public static class AuthHelper
{
    public static async Task<HttpClient> AuthenticateAsAdminAsync(CustomWebApplicationFactory factory)
    {
        var client = factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new
        {
            email = "admin@bookstore.com",
            password = "Admin@123"
        });
        loginResponse.EnsureSuccessStatusCode();

        var json = await loginResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var token = doc.RootElement.GetProperty("token").GetString()!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<HttpClient> AuthenticateAsEditorAsync(CustomWebApplicationFactory factory)
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/auth/register", new
        {
            email = $"editor-{Guid.NewGuid():N}@test.com",
            password = "TestPass123",
            name = "Test Editor",
            role = 1
        });
        registerResponse.EnsureSuccessStatusCode();

        var json = await registerResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var token = doc.RootElement.GetProperty("token").GetString()!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
