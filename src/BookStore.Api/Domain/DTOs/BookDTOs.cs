namespace BookStore.Api.Domain.DTOs;

public record CreateBookRequest(
    string Title,
    string Author,
    string Isbn,
    int Year,
    decimal Price,
    int Stock,
    int CategoryId);

public record UpdateBookRequest(
    string Title,
    string Author,
    string Isbn,
    int Year,
    decimal Price,
    int Stock,
    int CategoryId);

public record BookResponse(
    int Id,
    string Title,
    string Author,
    string Isbn,
    int Year,
    decimal Price,
    int Stock,
    int CategoryId,
    string CategoryName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
