using BookStore.Api.Domain.DTOs;
using BookStore.Api.Domain.Entities;
using BookStore.Api.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/categories")
            .WithTags("Categories")
            .RequireAuthorization();

        group.MapGet("/", (ICategoryService categoryService) =>
        {
            var categories = categoryService.GetAll();
            var response = categories.Select(ToResponse);
            return Results.Ok(response);
        })
        .Produces<IEnumerable<CategoryResponse>>(200)
        .Produces(401)
        .WithName("GetAllCategories")
        .WithOpenApi();

        group.MapGet("/{id:int}", (int id, ICategoryService categoryService) =>
        {
            var category = categoryService.GetById(id);
            if (category is null)
                return Results.NotFound(new { message = $"Category with ID {id} not found." });

            return Results.Ok(ToResponse(category));
        })
        .Produces<CategoryResponse>(200)
        .Produces(401)
        .Produces(404)
        .WithName("GetCategoryById")
        .WithOpenApi();

        group.MapPost("/", (CreateCategoryRequest request, ICategoryService categoryService) =>
        {
            var errors = ValidateCategory(request.Name, request.Description);
            if (errors.Count > 0)
                return Results.BadRequest(ValidationError.Create("Invalid category data.", "/categories", errors));

            if (categoryService.NameExists(request.Name))
                return Results.Conflict(new { message = $"Category '{request.Name}' already exists." });

            var category = new Category
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim()
            };

            var created = categoryService.Create(category);
            return Results.Created($"/categories/{created.Id}", ToResponse(created));
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
        .Produces<CategoryResponse>(201)
        .Produces<ValidationError>(400)
        .Produces(401)
        .Produces(403)
        .Produces(409)
        .WithName("CreateCategory")
        .WithOpenApi();

        group.MapPut("/{id:int}", (int id, UpdateCategoryRequest request, ICategoryService categoryService) =>
        {
            var category = categoryService.GetById(id);
            if (category is null)
                return Results.NotFound(new { message = $"Category with ID {id} not found." });

            var errors = ValidateCategory(request.Name, request.Description);
            if (errors.Count > 0)
                return Results.BadRequest(ValidationError.Create("Invalid category data.", $"/categories/{id}", errors));

            if (categoryService.NameExists(request.Name, id))
                return Results.Conflict(new { message = $"Category '{request.Name}' already exists." });

            category.Name = request.Name.Trim();
            category.Description = request.Description?.Trim();

            var updated = categoryService.Update(category);
            return Results.Ok(ToResponse(updated));
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
        .Produces<CategoryResponse>(200)
        .Produces<ValidationError>(400)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(409)
        .WithName("UpdateCategory")
        .WithOpenApi();

        group.MapDelete("/{id:int}", (int id, ICategoryService categoryService) =>
        {
            var category = categoryService.GetById(id);
            if (category is null)
                return Results.NotFound(new { message = $"Category with ID {id} not found." });

            if (categoryService.HasBooks(id))
                return Results.Conflict(new { message = "Cannot delete category with associated books." });

            categoryService.Delete(category);
            return Results.NoContent();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
        .Produces(204)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(409)
        .WithName("DeleteCategory")
        .WithOpenApi();
    }

    private static CategoryResponse ToResponse(Category category) =>
        new(category.Id, category.Name, category.Description, category.Books?.Count ?? 0);

    private static List<string> ValidateCategory(string name, string? description)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add("Name is required.");
        else if (name.Length > 100)
            errors.Add("Name must be at most 100 characters.");

        if (description?.Length > 500)
            errors.Add("Description must be at most 500 characters.");

        return errors;
    }
}
