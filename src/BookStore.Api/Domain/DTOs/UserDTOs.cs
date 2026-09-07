using BookStore.Api.Domain.Enums;

namespace BookStore.Api.Domain.DTOs;

public record CreateUserRequest(string Email, string Password, string Name, UserRole Role);

public record UserResponse(int Id, string Email, string Name, string Role, DateTime CreatedAt);
