using BookStore.Api.Domain.Entities;
using BookStore.Api.Domain.Services;
using BookStore.UnitTests.Helpers;

namespace BookStore.UnitTests.Services;

public class CategoryServiceTests
{
    [Fact]
    public void Create_ValidCategory_PersistsSuccessfully()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new CategoryService(context);
        var category = new Category { Name = "History", Description = "Historical books" };

        // Act
        var created = service.Create(category);

        // Assert
        Assert.NotEqual(0, created.Id);
        Assert.Equal("History", created.Name);
    }

    [Fact]
    public void GetAll_ReturnsAllCategoriesOrderedByName()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new CategoryService(context);
        service.Create(new Category { Name = "Zebra" });
        service.Create(new Category { Name = "Alpha" });

        // Act
        var all = service.GetAll();

        // Assert — sorted alphabetically, may include seeded categories
        Assert.True(all.Count >= 2);
        for (int i = 1; i < all.Count; i++)
            Assert.True(string.Compare(all[i - 1].Name, all[i].Name, StringComparison.Ordinal) <= 0);
    }

    [Fact]
    public void GetById_ExistingCategory_ReturnsCategoryWithBooks()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new CategoryService(context);
        var category = service.Create(new Category { Name = "Test" });

        // Act
        var result = service.GetById(category.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
        Assert.NotNull(result.Books); // collection loaded via Include
    }

    [Fact]
    public void GetById_NonExistentId_ReturnsNull()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new CategoryService(context);

        // Act & Assert
        Assert.Null(service.GetById(999));
    }

    [Fact]
    public void Update_ExistingCategory_PersistsChanges()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new CategoryService(context);
        var category = service.Create(new Category { Name = "Old Name", Description = "Old" });

        // Act
        category.Name = "New Name";
        category.Description = "Updated";
        var updated = service.Update(category);

        // Assert
        Assert.Equal("New Name", updated.Name);
        Assert.Equal("Updated", updated.Description);
    }

    [Fact]
    public void Delete_CategoryWithoutBooks_RemovesSuccessfully()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new CategoryService(context);
        var category = service.Create(new Category { Name = "ToDelete" });

        // Act
        service.Delete(category);

        // Assert
        Assert.Null(service.GetById(category.Id));
    }

    [Fact]
    public void NameExists_ExistingName_ReturnsTrue()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new CategoryService(context);
        service.Create(new Category { Name = "Unique" });

        // Act & Assert
        Assert.True(service.NameExists("unique")); // case-insensitive
    }

    [Fact]
    public void NameExists_WithExcludeId_ReturnsFalseForSameCategory()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new CategoryService(context);
        var category = service.Create(new Category { Name = "MyCategory" });

        // Act
        var exists = service.NameExists("mycategory", category.Id);

        // Assert — should not conflict with itself during update
        Assert.False(exists);
    }

    [Fact]
    public void NameExists_NonExistentName_ReturnsFalse()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new CategoryService(context);

        // Act & Assert
        Assert.False(service.NameExists("NonExistent"));
    }

    [Fact]
    public void HasBooks_CategoryWithBooks_ReturnsTrue()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new CategoryService(context);
        var category = service.Create(new Category { Name = "WithBooks" });

        context.Books.Add(new Book
        {
            Title = "Test", Author = "Author", Isbn = "1234567890",
            Year = 2023, Price = 10m, Stock = 1, CategoryId = category.Id
        });
        context.SaveChanges();

        // Act & Assert
        Assert.True(service.HasBooks(category.Id));
    }

    [Fact]
    public void HasBooks_CategoryWithoutBooks_ReturnsFalse()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new CategoryService(context);
        var category = service.Create(new Category { Name = "Empty" });

        // Act & Assert
        Assert.False(service.HasBooks(category.Id));
    }
}
