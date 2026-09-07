namespace BookStore.Api.Domain.DTOs;

public record CreateCategoryRequest(string Name, string? Description);

public record UpdateCategoryRequest(string Name, string? Description);

public record CategoryResponse(int Id, string Name, string? Description, int BookCount);
