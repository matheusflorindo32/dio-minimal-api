using BookStore.Api.Domain.Entities;
using BookStore.Api.Domain.Interfaces;
using BookStore.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Domain.Services;

public class BookService : IBookService
{
    private readonly AppDbContext _context;

    public BookService(AppDbContext context)
    {
        _context = context;
    }

    public (List<Book> Items, int TotalCount) GetAll(int page, int pageSize, string? title, string? author)
    {
        var query = _context.Books
            .Include(b => b.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(b => b.Title.ToLower().Contains(title.ToLower()));

        if (!string.IsNullOrWhiteSpace(author))
            query = query.Where(b => b.Author.ToLower().Contains(author.ToLower()));

        var totalCount = query.Count();

        var items = query
            .OrderBy(b => b.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, totalCount);
    }

    public Book? GetById(int id)
    {
        return _context.Books
            .Include(b => b.Category)
            .FirstOrDefault(b => b.Id == id);
    }

    public Book Create(Book book)
    {
        _context.Books.Add(book);
        _context.SaveChanges();

        return _context.Books
            .Include(b => b.Category)
            .First(b => b.Id == book.Id);
    }

    public Book Update(Book book)
    {
        book.UpdatedAt = DateTime.UtcNow;
        _context.Books.Update(book);
        _context.SaveChanges();

        return _context.Books
            .Include(b => b.Category)
            .First(b => b.Id == book.Id);
    }

    public void Delete(Book book)
    {
        _context.Books.Remove(book);
        _context.SaveChanges();
    }

    public bool IsbnExists(string isbn, int? excludeId = null)
    {
        var query = _context.Books.Where(b => b.Isbn == isbn);
        if (excludeId.HasValue)
            query = query.Where(b => b.Id != excludeId.Value);
        return query.Any();
    }
}
