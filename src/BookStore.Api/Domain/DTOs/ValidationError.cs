namespace BookStore.Api.Domain.DTOs;

public record ValidationError(string Type, string Title, int Status, string Detail, string Instance)
{
    public List<string> Errors { get; init; } = new();

    public static ValidationError Create(string detail, string instance, List<string> errors) =>
        new("https://tools.ietf.org/html/rfc9110#section-15.5.1",
            "One or more validation errors occurred.",
            400,
            detail,
            instance)
        { Errors = errors };
}
