namespace Company.Domain.Common.Exceptions;

/// <summary>
/// Represents the base exception for all domain-related errors.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Gets the type of domain error.
    /// </summary>
    public string ErrorType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public DomainException(string message)
        : base(message)
    {
        ErrorType = GetType().Name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error type and message.
    /// </summary>
    /// <param name="errorType">The type of domain error.</param>
    /// <param name="message">The error message.</param>
    public DomainException(string errorType, string message)
        : base(message)
    {
        ErrorType = errorType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorType = GetType().Name;
    }
}