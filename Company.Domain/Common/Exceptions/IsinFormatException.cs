namespace Company.Domain.Common.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an ISIN code format is invalid.
/// </summary>
public class IsinFormatException : BusinessRuleException
{
    /// <summary>
    /// Gets the invalid ISIN value.
    /// </summary>
    public string Isin { get; }

    private IsinFormatException(string code, string message, string isin)
        : base(code, message)
    {
        Isin = isin;
        Context["Isin"] = isin;
    }

    /// <summary>
    /// Creates an exception for an ISIN with invalid length.
    /// </summary>
    /// <param name="isin">The invalid ISIN value.</param>
    /// <returns>A <see cref="IsinFormatException"/> for invalid length.</returns>
    public static IsinFormatException InvalidLength(string isin)
    {
        return new IsinFormatException(
            "IsinInvalidLength",
            "ISIN must be exactly 12 characters long",
            isin);
    }

    /// <summary>
    /// Creates an exception for an ISIN with an invalid country code.
    /// </summary>
    /// <param name="isin">The invalid ISIN value.</param>
    /// <returns>A <see cref="IsinFormatException"/> for invalid country code.</returns>
    public static IsinFormatException InvalidCountryCode(string isin)
    {
        return new IsinFormatException(
            "IsinInvalidCountryCode",
            "The first 2 characters of ISIN must be uppercase alphabetic characters (A-Z)",
            isin);
    }

    /// <summary>
    /// Creates an exception for an ISIN with an invalid format.
    /// </summary>
    /// <param name="isin">The invalid ISIN value.</param>
    /// <returns>A <see cref="IsinFormatException"/> for invalid format.</returns>
    public static IsinFormatException InvalidFormat(string isin)
    {
        return new IsinFormatException(
            "IsinInvalidFormat",
            "ISIN must start with 2 alphabetic characters (A-Z), followed by 9 alphanumeric characters, and end with a numeric check digit",
            isin);
    }
}