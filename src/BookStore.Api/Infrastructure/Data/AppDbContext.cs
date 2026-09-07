using BookStore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Book configuration
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasIndex(b => b.Isbn).IsUnique();

            entity.HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Name).IsUnique();
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Fiction", Description = "Novels, short stories, and literary works" },
            new Category { Id = 2, Name = "Technology", Description = "Programming, software, and IT books" },
            new Category { Id = 3, Name = "Science", Description = "Scientific research and discovery" }
        );

        // Seed admin user with pre-hashed password: "Admin@123"
        // Salt: dGVzdHNhbHQxMjM0NTY= (base64 of "testsalt123456")
        // This is a development-only seed. Change in production.
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Email = "admin@bookstore.com",
                Name = "Admin",
                PasswordHash = "dGVzdHNhbHQxMjM0NTYZ.kXJG8sTO90qRpUGPHFxKyMnXNNnGqR/iLxVxqRV1v3c=",
                Role = "Admin",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<Book>().HasData(
            new Book
            {
                Id = 1,
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Isbn = "9780132350884",
                Year = 2008,
                Price = 39.99m,
                Stock = 15,
                CategoryId = 2,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Book
            {
                Id = 2,
                Title = "The Pragmatic Programmer",
                Author = "David Thomas & Andrew Hunt",
                Isbn = "9780135957059",
                Year = 2019,
                Price = 49.99m,
                Stock = 10,
                CategoryId = 2,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
