using BookStore.Api.Domain.Entities;
using BookStore.Api.Domain.Interfaces;
using BookStore.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Domain.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public List<Category> GetAll()
    {
        return _context.Categories
            .Include(c => c.Books)
            .OrderBy(c => c.Name)
            .ToList();
    }

    public Category? GetById(int id)
    {
        return _context.Categories
            .Include(c => c.Books)
            .FirstOrDefault(c => c.Id == id);
    }

    public Category Create(Category category)
    {
        _context.Categories.Add(category);
        _context.SaveChanges();
        return category;
    }

    public Category Update(Category category)
    {
        _context.Categories.Update(category);
        _context.SaveChanges();
        return category;
    }

    public void Delete(Category category)
    {
        _context.Categories.Remove(category);
        _context.SaveChanges();
    }

    public bool NameExists(string name, int? excludeId = null)
    {
        var query = _context.Categories.Where(c => c.Name.ToLower() == name.ToLower());
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return query.Any();
    }

    public bool HasBooks(int id)
    {
        return _context.Books.Any(b => b.CategoryId == id);
    }
}
