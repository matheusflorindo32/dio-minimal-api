using BookStore.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.UnitTests.Helpers;

public static class TestDbContextFactory
{
    /// <summary>
    /// Creates an in-memory AppDbContext for isolated unit testing.
    /// Each call uses a unique database name to prevent test interference.
    /// </summary>
    public static AppDbContext Create(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
