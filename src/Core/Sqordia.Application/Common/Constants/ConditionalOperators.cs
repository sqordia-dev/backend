namespace Sqordia.Application.Common.Constants;

/// <summary>
/// Constants for conditional logic operators used in QuestionTemplate.ConditionalLogic JSON.
/// Eliminates magic strings in the conditional logic evaluator.
/// </summary>
public static class ConditionalOperators
{
    // Comparison operators
    public new const string Equals = "equals";
    public const string NotEquals = "notEquals";
    public const string Contains = "contains";
    public const string NotContains = "notContains";
    public const string GreaterThan = "greaterThan";
    public const string LessThan = "lessThan";
    public const string GreaterThanOrEqual = "greaterThanOrEqual";
    public const string LessThanOrEqual = "lessThanOrEqual";
    public const string IsEmpty = "isEmpty";
    public const string IsNotEmpty = "isNotEmpty";

    // Logical operators
    public const string And = "AND";
    public const string Or = "OR";

    // JSON property names
    public const string ShowIfProperty = "showIf";
    public const string EqualsProperty = "equals";
    public const string OperatorProperty = "operator";
    public const string ConditionsProperty = "conditions";
    public const string FieldProperty = "field";
    public const string ValueProperty = "value";
}
