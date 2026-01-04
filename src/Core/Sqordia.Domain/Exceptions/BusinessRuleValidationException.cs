namespace Sqordia.Domain.Exceptions;

public class ValidationFailure
{
    public string PropertyName { get; set; } = null!;
    public string ErrorMessage { get; set; } = null!;
    public object AttemptedValue { get; set; } = null!;
    public string ErrorCode { get; set; } = null!;

    public ValidationFailure(string propertyName, string errorMessage)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
    }

    public ValidationFailure(string propertyName, string errorMessage, object attemptedValue)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        AttemptedValue = attemptedValue;
    }
}

/// <summary>
/// Exception thrown when business rule validation fails
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    public IEnumerable<ValidationFailure> Errors { get; }

    public BusinessRuleValidationException(IEnumerable<ValidationFailure> errors)
        : base("Business rule validation failed")
    {
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
    }

    public BusinessRuleValidationException(string message)
        : base(message)
    {
        Errors = Array.Empty<ValidationFailure>();
    }

    public BusinessRuleValidationException(string message, IEnumerable<ValidationFailure> errors)
        : base(message)
    {
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
    }
}
