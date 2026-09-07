namespace BookStore.Api.Domain.DTOs;

public record LoginResponse(string Email, string Name, string Role, string Token);
