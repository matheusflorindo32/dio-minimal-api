using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BookStore.IntegrationTests.Helpers;

namespace BookStore.IntegrationTests.Endpoints;

public class CategoryEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CategoryEndpointsTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetAll_Authenticated_Returns200()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);

        // Act
        var response = await client.GetAsync("/categories");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/categories");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsAdmin_Returns201()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var payload = new { name = $"Cat-{Guid.NewGuid():N}", description = "Test category" };

        // Act
        var response = await client.PostAsJsonAsync("/categories", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("id", out _));
    }

    [Fact]
    public async Task Create_AsEditor_Returns403()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsEditorAsync(_factory);
        var payload = new { name = "Forbidden Cat", description = "Should fail" };

        // Act
        var response = await client.PostAsJsonAsync("/categories", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateName_Returns409()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var name = $"Dup-{Guid.NewGuid():N}";
        await client.PostAsJsonAsync("/categories", new { name, description = "First" });

        // Act
        var response = await client.PostAsJsonAsync("/categories", new { name, description = "Second" });

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyName_Returns400()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);

        // Act
        var response = await client.PostAsJsonAsync("/categories", new { name = "", description = "" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingCategory_Returns200()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var createResp = await client.PostAsJsonAsync("/categories",
            new { name = $"Get-{Guid.NewGuid():N}", description = "Find me" });
        var json = await createResp.Content.ReadAsStringAsync();
        var id = JsonDocument.Parse(json).RootElement.GetProperty("id").GetInt32();

        // Act
        var response = await client.GetAsync($"/categories/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_NonExistent_Returns404()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);

        // Act
        var response = await client.GetAsync("/categories/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingCategory_Returns200()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var createResp = await client.PostAsJsonAsync("/categories",
            new { name = $"Upd-{Guid.NewGuid():N}", description = "Before" });
        var json = await createResp.Content.ReadAsStringAsync();
        var id = JsonDocument.Parse(json).RootElement.GetProperty("id").GetInt32();

        // Act
        var response = await client.PutAsJsonAsync($"/categories/{id}",
            new { name = $"Updated-{Guid.NewGuid():N}", description = "After" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_CategoryWithoutBooks_Returns204()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var createResp = await client.PostAsJsonAsync("/categories",
            new { name = $"Del-{Guid.NewGuid():N}", description = "Delete me" });
        var json = await createResp.Content.ReadAsStringAsync();
        var id = JsonDocument.Parse(json).RootElement.GetProperty("id").GetInt32();

        // Act
        var response = await client.DeleteAsync($"/categories/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
