using BookStore.Api.Domain.Entities;
using BookStore.Api.Domain.Services;
using BookStore.UnitTests.Helpers;

namespace BookStore.UnitTests.Services;

public class BookServiceTests
{
    private static Category SeedCategory(Api.Infrastructure.Data.AppDbContext context)
    {
        var category = new Category { Name = "TestCat", Description = "Test" };
        context.Categories.Add(category);
        context.SaveChanges();
        return category;
    }

    private static Book CreateSampleBook(int categoryId, string isbn = "1234567890") => new()
    {
        Title = "Test Book",
        Author = "Test Author",
        Isbn = isbn,
        Year = 2023,
        Price = 29.99m,
        Stock = 5,
        CategoryId = categoryId
    };

    [Fact]
    public void Create_ValidBook_PersistsAndReturnsWithCategory()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new BookService(context);
        var category = SeedCategory(context);
        var book = CreateSampleBook(category.Id);

        // Act
        var created = service.Create(book);

        // Assert
        Assert.NotEqual(0, created.Id);
        Assert.Equal("Test Book", created.Title);
        Assert.NotNull(created.Category);
        Assert.Equal("TestCat", created.Category.Name);
    }

    [Fact]
    public void GetById_ExistingBook_ReturnsBookWithCategory()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new BookService(context);
        var category = SeedCategory(context);
        var created = service.Create(CreateSampleBook(category.Id));

        // Act
        var result = service.GetById(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.NotNull(result.Category);
    }

    [Fact]
    public void GetById_NonExistentId_ReturnsNull()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new BookService(context);

        // Act & Assert
        Assert.Null(service.GetById(999));
    }

    [Fact]
    public void GetAll_ReturnsPaginatedResults()
    {
        // Arrange — isolate pagination from the application's seeded books.
        using var context = TestDbContextFactory.Create();
        context.Books.RemoveRange(context.Books);
        context.SaveChanges();

        var service = new BookService(context);
        var category = SeedCategory(context);
        for (int i = 0; i < 25; i++)
            service.Create(CreateSampleBook(category.Id, $"ISBN{i:D10}"));

        // Act
        var (page1, total) = service.GetAll(1, 10, null, null);
        var (page2, _) = service.GetAll(2, 10, null, null);
        var (page3, _) = service.GetAll(3, 10, null, null);

        // Assert
        Assert.Equal(25, total);
        Assert.Equal(10, page1.Count);
        Assert.Equal(10, page2.Count);
        Assert.Equal(5, page3.Count);
    }

    [Fact]
    public void GetAll_FilterByTitle_ReturnsMatchingBooks()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new BookService(context);
        var category = SeedCategory(context);
        service.Create(new Book { Title = "C# in Depth", Author = "Jon Skeet", Isbn = "1111111111", Year = 2019, Price = 40m, Stock = 3, CategoryId = category.Id });
        service.Create(new Book { Title = "Python Crash Course", Author = "Eric Matthes", Isbn = "2222222222", Year = 2019, Price = 35m, Stock = 7, CategoryId = category.Id });

        // Act
        var (results, count) = service.GetAll(1, 10, "C#", null);

        // Assert
        Assert.Single(results);
        Assert.Equal("C# in Depth", results[0].Title);
    }

    [Fact]
    public void GetAll_FilterByAuthor_ReturnsMatchingBooks()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new BookService(context);
        var category = SeedCategory(context);
        service.Create(new Book { Title = "Book A", Author = "Alice Smith", Isbn = "3333333333", Year = 2020, Price = 20m, Stock = 1, CategoryId = category.Id });
        service.Create(new Book { Title = "Book B", Author = "Bob Jones", Isbn = "4444444444", Year = 2021, Price = 25m, Stock = 2, CategoryId = category.Id });

        // Act
        var (results, _) = service.GetAll(1, 10, null, "alice");

        // Assert
        Assert.Single(results);
        Assert.Equal("Alice Smith", results[0].Author);
    }

    [Fact]
    public void Update_ExistingBook_UpdatesFieldsAndTimestamp()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new BookService(context);
        var category = SeedCategory(context);
        var book = service.Create(CreateSampleBook(category.Id));
        var originalUpdatedAt = book.UpdatedAt;

        // Act
        book.Title = "Updated Title";
        book.Price = 99.99m;
        var updated = service.Update(book);

        // Assert
        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal(99.99m, updated.Price);
        Assert.NotNull(updated.UpdatedAt);
        Assert.NotEqual(originalUpdatedAt, updated.UpdatedAt);
    }

    [Fact]
    public void Delete_ExistingBook_RemovesFromDatabase()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new BookService(context);
        var category = SeedCategory(context);
        var book = service.Create(CreateSampleBook(category.Id));

        // Act
        service.Delete(book);

        // Assert
        Assert.Null(service.GetById(book.Id));
    }

    [Fact]
    public void IsbnExists_ExistingIsbn_ReturnsTrue()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new BookService(context);
        var category = SeedCategory(context);
        service.Create(CreateSampleBook(category.Id, "9999999999"));

        // Act & Assert
        Assert.True(service.IsbnExists("9999999999"));
    }

    [Fact]
    public void IsbnExists_WithExcludeId_ReturnsFalseForSameBook()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new BookService(context);
        var category = SeedCategory(context);
        var book = service.Create(CreateSampleBook(category.Id, "8888888888"));

        // Act — exclude the book's own ID (used during updates)
        var exists = service.IsbnExists("8888888888", book.Id);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void IsbnExists_NonExistentIsbn_ReturnsFalse()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new BookService(context);

        // Act & Assert
        Assert.False(service.IsbnExists("0000000000"));
    }
}
