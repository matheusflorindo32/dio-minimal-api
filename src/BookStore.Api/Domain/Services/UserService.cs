using BookStore.Api.Domain.DTOs;
using BookStore.Api.Domain.Entities;
using BookStore.Api.Domain.Interfaces;
using BookStore.Api.Infrastructure.Data;

namespace BookStore.Api.Domain.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private const int PageSize = 10;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public User? Login(LoginRequest request)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null) return null;

        var valid = BCryptVerify(request.Password, user.PasswordHash);
        return valid ? user : null;
    }

    public User Create(CreateUserRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCryptHash(request.Password),
            Name = request.Name,
            Role = request.Role.ToString()
        };

        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }

    public User? GetById(int id)
    {
        return _context.Users.FirstOrDefault(u => u.Id == id);
    }

    public List<User> GetAll(int page)
    {
        return _context.Users
            .OrderBy(u => u.Id)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }

    public bool EmailExists(string email)
    {
        return _context.Users.Any(u => u.Email == email);
    }

    // Simple password hashing using HMACSHA256 with a salt
    // In production, use BCrypt via a NuGet package. Here we use a built-in approach
    // to avoid adding an extra dependency for a learning project.
    private static string BCryptHash(string password)
    {
        var salt = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));
        var hash = ComputeHash(password, salt);
        return $"{salt}.{hash}";
    }

    private static bool BCryptVerify(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;
        var hash = ComputeHash(password, parts[0]);
        return hash == parts[1];
    }

    private static string ComputeHash(string password, string salt)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(salt));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hash);
    }
}
