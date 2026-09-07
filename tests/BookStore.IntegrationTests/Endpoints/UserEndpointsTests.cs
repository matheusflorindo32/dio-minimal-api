using System.Net;
using BookStore.IntegrationTests.Helpers;

namespace BookStore.IntegrationTests.Endpoints;

public class UserEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public UserEndpointsTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetAll_AsAdmin_Returns200()
    {
        var client = await AuthHelper.AuthenticateAsAdminAsync(_factory);
        var response = await client.GetAsync("/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AsEditor_Returns403()
    {
        var client = await AuthHelper.AuthenticateAsEditorAsync(_factory);
        var response = await client.GetAsync("/users");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/users");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
