using BookStore.Api.Domain.DTOs;
using BookStore.Api.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        group.MapGet("/", (int? page, IUserService userService) =>
        {
            var currentPage = page ?? 1;
            if (currentPage < 1) currentPage = 1;

            var users = userService.GetAll(currentPage);
            var response = users.Select(u => new UserResponse(u.Id, u.Email, u.Name, u.Role, u.CreatedAt));
            return Results.Ok(response);
        })
        .Produces<IEnumerable<UserResponse>>(200)
        .Produces(401)
        .Produces(403)
        .WithName("GetAllUsers")
        .WithOpenApi();

        group.MapGet("/{id:int}", (int id, IUserService userService) =>
        {
            var user = userService.GetById(id);
            if (user is null)
                return Results.NotFound(new { message = $"User with ID {id} not found." });

            return Results.Ok(new UserResponse(user.Id, user.Email, user.Name, user.Role, user.CreatedAt));
        })
        .Produces<UserResponse>(200)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .WithName("GetUserById")
        .WithOpenApi();
    }
}
