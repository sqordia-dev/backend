namespace Sqordia.Application.Common.Models;

public enum ErrorType
{
    Failure = 0,
    Validation,
    NotFound,
    Unauthorized,
    Forbidden,
    Conflict,
    NotImplemented,
    InternalServerError
}

public class Error
{
    public string Code { get; }
    public string Message { get; }
    public string? Details { get; }
    public ErrorType Type { get; }

    public Error(string code, string message, string? details = null)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Details = details;
        Type = ErrorType.Failure;
    }

    private Error(string code, string message, ErrorType type, string? details = null)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Details = details;
        Type = type;
    }

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);

    public static Error NotImplemented(string code, string message) =>
        new(code, message, ErrorType.NotImplemented);

    public static Error InternalServerError(string code, string message) =>
        new(code, message, ErrorType.InternalServerError);
}
