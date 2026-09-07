using BookStore.Api.Domain.DTOs;
using BookStore.Api.Domain.Entities;
using BookStore.Api.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.Api.Endpoints;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/books")
            .WithTags("Books")
            .RequireAuthorization();

        group.MapGet("/", (int? page, int? pageSize, string? title, string? author, IBookService bookService) =>
        {
            var currentPage = Math.Max(page ?? 1, 1);
            var currentPageSize = Math.Clamp(pageSize ?? 10, 1, 50);

            var (items, totalCount) = bookService.GetAll(currentPage, currentPageSize, title, author);

            var totalPages = (int)Math.Ceiling(totalCount / (double)currentPageSize);

            var response = new PagedResponse<BookResponse>(
                items.Select(ToResponse),
                currentPage,
                currentPageSize,
                totalCount,
                totalPages);

            return Results.Ok(response);
        })
        .Produces<PagedResponse<BookResponse>>(200)
        .Produces(401)
        .WithName("GetAllBooks")
        .WithOpenApi();

        group.MapGet("/{id:int}", (int id, IBookService bookService) =>
        {
            var book = bookService.GetById(id);
            if (book is null)
                return Results.NotFound(new { message = $"Book with ID {id} not found." });

            return Results.Ok(ToResponse(book));
        })
        .Produces<BookResponse>(200)
        .Produces(401)
        .Produces(404)
        .WithName("GetBookById")
        .WithOpenApi();

        group.MapPost("/", (CreateBookRequest request, IBookService bookService, ICategoryService categoryService) =>
        {
            var errors = ValidateBook(request.Title, request.Author, request.Isbn, request.Year, request.Price, request.Stock);

            if (errors.Count > 0)
                return Results.BadRequest(ValidationError.Create("Invalid book data.", "/books", errors));

            if (bookService.IsbnExists(request.Isbn))
                return Results.Conflict(new { message = $"A book with ISBN '{request.Isbn}' already exists." });

            if (categoryService.GetById(request.CategoryId) is null)
                return Results.BadRequest(ValidationError.Create("Invalid category.", "/books",
                    new List<string> { $"Category with ID {request.CategoryId} not found." }));

            var book = new Book
            {
                Title = request.Title.Trim(),
                Author = request.Author.Trim(),
                Isbn = request.Isbn.Trim(),
                Year = request.Year,
                Price = request.Price,
                Stock = request.Stock,
                CategoryId = request.CategoryId
            };

            var created = bookService.Create(book);
            return Results.Created($"/books/{created.Id}", ToResponse(created));
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Editor" })
        .Produces<BookResponse>(201)
        .Produces<ValidationError>(400)
        .Produces(401)
        .Produces(403)
        .Produces(409)
        .WithName("CreateBook")
        .WithOpenApi();

        group.MapPut("/{id:int}", (int id, UpdateBookRequest request, IBookService bookService, ICategoryService categoryService) =>
        {
            var book = bookService.GetById(id);
            if (book is null)
                return Results.NotFound(new { message = $"Book with ID {id} not found." });

            var errors = ValidateBook(request.Title, request.Author, request.Isbn, request.Year, request.Price, request.Stock);

            if (errors.Count > 0)
                return Results.BadRequest(ValidationError.Create("Invalid book data.", $"/books/{id}", errors));

            if (bookService.IsbnExists(request.Isbn, id))
                return Results.Conflict(new { message = $"A book with ISBN '{request.Isbn}' already exists." });

            if (categoryService.GetById(request.CategoryId) is null)
                return Results.BadRequest(ValidationError.Create("Invalid category.", $"/books/{id}",
                    new List<string> { $"Category with ID {request.CategoryId} not found." }));

            book.Title = request.Title.Trim();
            book.Author = request.Author.Trim();
            book.Isbn = request.Isbn.Trim();
            book.Year = request.Year;
            book.Price = request.Price;
            book.Stock = request.Stock;
            book.CategoryId = request.CategoryId;

            var updated = bookService.Update(book);
            return Results.Ok(ToResponse(updated));
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
        .Produces<BookResponse>(200)
        .Produces<ValidationError>(400)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(409)
        .WithName("UpdateBook")
        .WithOpenApi();

        group.MapDelete("/{id:int}", (int id, IBookService bookService) =>
        {
            var book = bookService.GetById(id);
            if (book is null)
                return Results.NotFound(new { message = $"Book with ID {id} not found." });

            bookService.Delete(book);
            return Results.NoContent();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
        .Produces(204)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .WithName("DeleteBook")
        .WithOpenApi();
    }

    private static BookResponse ToResponse(Book book) =>
        new(book.Id, book.Title, book.Author, book.Isbn, book.Year,
            book.Price, book.Stock, book.CategoryId,
            book.Category?.Name ?? "", book.CreatedAt, book.UpdatedAt);

    private static List<string> ValidateBook(string title, string author, string isbn, int year, decimal price, int stock)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(title))
            errors.Add("Title is required.");
        else if (title.Length > 200)
            errors.Add("Title must be at most 200 characters.");

        if (string.IsNullOrWhiteSpace(author))
            errors.Add("Author is required.");
        else if (author.Length > 150)
            errors.Add("Author must be at most 150 characters.");

        if (string.IsNullOrWhiteSpace(isbn))
            errors.Add("ISBN is required.");
        else if (isbn.Length is < 10 or > 13)
            errors.Add("ISBN must be between 10 and 13 characters.");

        if (year < 1450 || year > DateTime.UtcNow.Year + 1)
            errors.Add($"Year must be between 1450 and {DateTime.UtcNow.Year + 1}.");

        if (price < 0)
            errors.Add("Price cannot be negative.");

        if (stock < 0)
            errors.Add("Stock cannot be negative.");

        return errors;
    }
}
