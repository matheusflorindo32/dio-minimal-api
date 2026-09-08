using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookStore.Api.Domain.DTOs;
using BookStore.Api.Domain.Enums;
using BookStore.Api.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace BookStore.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Authentication");

        group.MapPost("/login", (LoginRequest request, IUserService userService, IConfiguration config) =>
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(request.Email))
                errors.Add("Email is required.");
            if (string.IsNullOrWhiteSpace(request.Password))
                errors.Add("Password is required.");

            if (errors.Count > 0)
                return Results.BadRequest(ValidationError.Create("Invalid login request.", "/auth/login", errors));

            var user = userService.Login(request);
            if (user is null)
                return Results.Unauthorized();

            var token = GenerateToken(user.Email, user.Name, user.Role, config);

            return Results.Ok(new LoginResponse(user.Email, user.Name, user.Role, token));
        })
        .AllowAnonymous()
        .Produces<LoginResponse>(200)
        .Produces(401)
        .Produces<ValidationError>(400)
        .WithName("Login")
        .WithOpenApi();

        group.MapPost("/register", (CreateUserRequest request, IUserService userService, IConfiguration config) =>
        {
            var errors = ValidateCreateUser(request);
            if (errors.Count > 0)
                return Results.BadRequest(ValidationError.Create("Invalid registration data.", "/auth/register", errors));

            if (userService.EmailExists(request.Email))
                return Results.Conflict(new { message = "Email already registered." });

            // Public registration must never grant administrative privileges.
            // The seeded development admin is the only bootstrap Admin in this educational project.
            var safeRequest = request with { Role = UserRole.Editor };
            var user = userService.Create(safeRequest);
            var token = GenerateToken(user.Email, user.Name, user.Role, config);

            return Results.Created($"/users/{user.Id}",
                new LoginResponse(user.Email, user.Name, user.Role, token));
        })
        .AllowAnonymous()
        .Produces<LoginResponse>(201)
        .Produces<ValidationError>(400)
        .Produces(409)
        .WithName("Register")
        .WithOpenApi();
    }

    private static string GenerateToken(string email, string name, string role, IConfiguration config)
    {
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static List<string> ValidateCreateUser(CreateUserRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email is required.");
        else if (!request.Email.Contains('@') || !request.Email.Contains('.'))
            errors.Add("Email format is invalid.");

        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("Password is required.");
        else if (request.Password.Length < 6)
            errors.Add("Password must be at least 6 characters.");

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Name is required.");
        else if (request.Name.Length > 100)
            errors.Add("Name must be at most 100 characters.");

        return errors;
    }
}
