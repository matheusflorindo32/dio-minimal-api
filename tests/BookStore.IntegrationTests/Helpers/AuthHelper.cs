using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookStore.IntegrationTests.Helpers;

/// <summary>
/// Helper to register and authenticate a test user, then set the JWT token on the HttpClient.
/// </summary>
public static class AuthHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<HttpClient> AuthenticateAsAdminAsync(CustomWebApplicationFactory factory)
    {
        var client = factory.CreateClient();

        // Register an admin user
        var registerPayload = new
        {
            email = $"admin-{Guid.NewGuid():N}@test.com",
            password = "TestPass123",
            name = "Test Admin",
            role = 0 // Admin
        };

        var registerResponse = await client.PostAsJsonAsync("/auth/register", registerPayload);
        registerResponse.EnsureSuccessStatusCode();

        var json = await registerResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var token = doc.RootElement.GetProperty("token").GetString()!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<HttpClient> AuthenticateAsEditorAsync(CustomWebApplicationFactory factory)
    {
        var client = factory.CreateClient();

        var registerPayload = new
        {
            email = $"editor-{Guid.NewGuid():N}@test.com",
            password = "TestPass123",
            name = "Test Editor",
            role = 1 // Editor
        };

        var registerResponse = await client.PostAsJsonAsync("/auth/register", registerPayload);
        registerResponse.EnsureSuccessStatusCode();

        var json = await registerResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var token = doc.RootElement.GetProperty("token").GetString()!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
