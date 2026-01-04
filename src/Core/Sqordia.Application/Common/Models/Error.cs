namespace Sqordia.Application.Common.Models;

public class Error
{
    public string Code { get; }
    public string Message { get; }
    public string? Details { get; }

    public Error(string code, string message, string? details = null)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Details = details;
    }

    public static Error NotFound(string code, string message) =>
        new(code, message);

    public static Error Validation(string code, string message) =>
        new(code, message);

    public static Error Unauthorized(string code, string message) =>
        new(code, message);

    public static Error Forbidden(string code, string message) =>
        new(code, message);

    public static Error Conflict(string code, string message) =>
        new(code, message);

    public static Error Failure(string code, string message) =>
        new(code, message);

    public static Error NotImplemented(string code, string message) =>
        new(code, message);

    public static Error InternalServerError(string code, string message) =>
        new(code, message);
}
