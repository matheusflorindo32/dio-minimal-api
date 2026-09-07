using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BookStore.IntegrationTests.Helpers;

namespace BookStore.IntegrationTests.Endpoints;

public class BookEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public BookEndpointsTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<int> CreateCategoryAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/categories",
            new { name = $"BookTestCat-{Guid.NewGuid():N}", description = "For book tests" });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement.GetProperty("id").GetInt32();
    }

    private static object BookPayload(int categoryId, string? isbn = null) => new
    {
        title = "Test Book",
        author = "Test Author",
        isbn = isbn ?? $"ISBN{Guid.NewGuid().ToString("N")[..10]}",
        year = 2023,
        price = 29.99,
        stock = 5,
        categoryId
    };

    [Fact]
    public async Task GetAll_Authenticated_Returns200WithPagination()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);

        // Act
        var response = await client.GetAsync("/books");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("page", out _));
        Assert.True(doc.RootElement.TryGetProperty("totalCount", out _));
    }

    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/books");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsAdmin_Returns201()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var catId = await CreateCategoryAsync(client);

        // Act
        var response = await client.PostAsJsonAsync("/books", BookPayload(catId));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsEditor_Returns201()
    {
        // Arrange — Editors can create books (Admin,Editor role)
        var adminClient = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var catId = await CreateCategoryAsync(adminClient);

        var editorClient = await AuthHelper.AuthenticateAsEditorAsync(_factory);

        // Act
        var response = await editorClient.PostAsJsonAsync("/books", BookPayload(catId));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateIsbn_Returns409()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var catId = await CreateCategoryAsync(client);
        var isbn = $"DUP{Guid.NewGuid().ToString("N")[..9]}";
        await client.PostAsJsonAsync("/books", BookPayload(catId, isbn));

        // Act
        var response = await client.PostAsJsonAsync("/books", BookPayload(catId, isbn));

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidCategory_Returns400()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);

        // Act
        var response = await client.PostAsJsonAsync("/books", BookPayload(99999));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_MissingTitle_Returns400()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var catId = await CreateCategoryAsync(client);

        // Act
        var response = await client.PostAsJsonAsync("/books", new
        {
            title = "",
            author = "Author",
            isbn = "1234567890",
            year = 2023,
            price = 10.0,
            stock = 1,
            categoryId = catId
        });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingBook_Returns200()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var catId = await CreateCategoryAsync(client);
        var createResp = await client.PostAsJsonAsync("/books", BookPayload(catId));
        var json = await createResp.Content.ReadAsStringAsync();
        var id = JsonDocument.Parse(json).RootElement.GetProperty("id").GetInt32();

        // Act
        var response = await client.GetAsync($"/books/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_NonExistent_Returns404()
    {
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var response = await client.GetAsync("/books/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_AsAdmin_Returns200()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var catId = await CreateCategoryAsync(client);
        var createResp = await client.PostAsJsonAsync("/books", BookPayload(catId));
        var json = await createResp.Content.ReadAsStringAsync();
        var id = JsonDocument.Parse(json).RootElement.GetProperty("id").GetInt32();

        // Act
        var response = await client.PutAsJsonAsync($"/books/{id}", new
        {
            title = "Updated Title",
            author = "Updated Author",
            isbn = $"UPD{Guid.NewGuid().ToString("N")[..10]}",
            year = 2024,
            price = 49.99,
            stock = 10,
            categoryId = catId
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_AsEditor_Returns403()
    {
        // Arrange — only Admin can update
        var adminClient = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var catId = await CreateCategoryAsync(adminClient);
        var createResp = await adminClient.PostAsJsonAsync("/books", BookPayload(catId));
        var json = await createResp.Content.ReadAsStringAsync();
        var id = JsonDocument.Parse(json).RootElement.GetProperty("id").GetInt32();

        var editorClient = await AuthHelper.AuthenticateAsEditorAsync(_factory);

        // Act
        var response = await editorClient.PutAsJsonAsync($"/books/{id}", new
        {
            title = "Hacked",
            author = "Hacker",
            isbn = "0000000000",
            year = 2024,
            price = 0,
            stock = 0,
            categoryId = catId
        });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AsAdmin_Returns204()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var catId = await CreateCategoryAsync(client);
        var createResp = await client.PostAsJsonAsync("/books", BookPayload(catId));
        var json = await createResp.Content.ReadAsStringAsync();
        var id = JsonDocument.Parse(json).RootElement.GetProperty("id").GetInt32();

        // Act
        var response = await client.DeleteAsync($"/books/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's gone
        var getResp = await client.GetAsync($"/books/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResp.StatusCode);
    }

    [Fact]
    public async Task Delete_AsEditor_Returns403()
    {
        // Arrange
        var adminClient = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var catId = await CreateCategoryAsync(adminClient);
        var createResp = await adminClient.PostAsJsonAsync("/books", BookPayload(catId));
        var json = await createResp.Content.ReadAsStringAsync();
        var id = JsonDocument.Parse(json).RootElement.GetProperty("id").GetInt32();

        var editorClient = await AuthHelper.AuthenticateAsEditorAsync(_factory);

        // Act
        var response = await editorClient.DeleteAsync($"/books/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithTitleFilter_ReturnsFilteredResults()
    {
        // Arrange
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var catId = await CreateCategoryAsync(client);
        var uniqueTitle = $"UniqueBook-{Guid.NewGuid():N}";
        await client.PostAsJsonAsync("/books", new
        {
            title = uniqueTitle,
            author = "FilterTest",
            isbn = $"FLT{Guid.NewGuid().ToString("N")[..10]}",
            year = 2023,
            price = 10.0,
            stock = 1,
            categoryId = catId
        });

        // Act
        var response = await client.GetAsync($"/books?title={uniqueTitle}");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, doc.RootElement.GetProperty("totalCount").GetInt32());
    }
}
