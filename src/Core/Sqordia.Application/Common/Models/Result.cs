namespace Sqordia.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }
    public Error[]? Errors { get; }

    protected Result(bool isSuccess, Error? error = null, Error[]? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors;
    }

    public static Result Success() => new(true);

    public static Result Failure(Error error) => new(false, error);

    public static Result Failure(string errorMessage) => new(false, new Error("General.Failure", errorMessage));

    public static Result Failure(IEnumerable<Error> errors) => new(false, errors: errors.ToArray());

    public static Result<T> Success<T>(T value) => new(value, true);

    public static Result<T> Failure<T>(Error error) => new(default!, false, error);

    public static Result<T> Failure<T>(string errorMessage) => new(default!, false, new Error("General.Failure", errorMessage));

    public static Result<T> Failure<T>(IEnumerable<Error> errors) => new(default!, false, errors: errors.ToArray());
}

public class Result<T> : Result
{
    public T? Value { get; }

    protected internal Result(T? value, bool isSuccess, Error? error = null, Error[]? errors = null)
        : base(isSuccess, error, errors)
    {
        Value = value;
    }
}
