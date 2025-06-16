namespace Company.Domain.Common.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a business rule is violated.
/// </summary>
public class BusinessRuleException : DomainException
{
    /// <summary>
    /// Gets the rule or property that caused the exception.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets additional context data about the exception.
    /// </summary>
    public Dictionary<string, string> Context { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class.
    /// </summary>
    /// <param name="code">The code of the business rule or property.</param>
    /// <param name="message">The exception message.</param>
    public BusinessRuleException(string code, string message)
        : base("BusinessRule", message)
    {
        Code = code;
        Context = new Dictionary<string, string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class with context.
    /// </summary>
    /// <param name="code">The code of the business rule or property.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="context">Additional context data.</param>
    public BusinessRuleException(string code, string message, Dictionary<string, string> context)
        : base("BusinessRule", message)
    {
        Code = code;
        Context = context;
    }

    /// <summary>
    /// Creates an exception for a uniqueness constraint violation.
    /// </summary>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value that violates the uniqueness constraint.</param>
    /// <returns>A <see cref="BusinessRuleException"/> for uniqueness violation.</returns>
    public static BusinessRuleException UniqueConstraintViolation(string property, string value)
    {
        var context = new Dictionary<string, string> { { "PropertyValue", value } };
        return new BusinessRuleException(
            $"Unique{property}",
            $"The {property} '{value}' already exists and must be unique",
            context);
    }

    /// <summary>
    /// Creates an exception for a field validation error.
    /// </summary>
    /// <param name="entityType">The type of the entity.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="reason">The reason for the validation error.</param>
    /// <returns>A <see cref="BusinessRuleException"/> for field validation error.</returns>
    public static BusinessRuleException InvalidField(string entityType, string fieldName, string reason)
    {
        var context = new Dictionary<string, string> {
            { "EntityType", entityType },
            { "FieldName", fieldName }
        };

        return new BusinessRuleException(
            $"Invalid{fieldName}",
            $"The {fieldName} field on {entityType} is invalid: {reason}",
            context);
    }

    /// <summary>
    /// Creates an exception for a required field that's missing.
    /// </summary>
    /// <param name="entityType">The type of the entity.</param>
    /// <param name="fieldName">The name of the required field.</param>
    /// <returns>A <see cref="BusinessRuleException"/> for a required field.</returns>
    public static BusinessRuleException RequiredField(string entityType, string fieldName)
    {
        var context = new Dictionary<string, string> {
            { "EntityType", entityType },
            { "FieldName", fieldName }
        };

        return new BusinessRuleException(
            $"Required{fieldName}",
            $"The {fieldName} field is required for {entityType}",
            context);
    }
}