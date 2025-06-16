using System.Text.RegularExpressions;
using Company.Domain.Common;
using Company.Domain.Common.Exceptions;

namespace Company.Domain.Entities;

/// <summary>
/// Represents a company entity in the domain.
/// </summary>
public class Company
{
    // Regular expression for complete ISIN validation: 2 alpha + 9 alphanumeric + 1 numeric
    private static readonly Regex IsinRegex = new Regex("^[A-Z]{2}[A-Z0-9]{9}[0-9]$", RegexOptions.Compiled);

    // Regular expression specifically for country code validation
    private static readonly Regex CountryCodeRegex = new Regex("^[A-Z]{2}", RegexOptions.Compiled);

    // Private constructor to enforce creation through factory method
    private Company()
    {
    }

    /// <summary>
    /// Gets the unique identifier of the company.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the name of the company.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the stock ticker symbol of the company.
    /// </summary>
    public string Ticker { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the stock exchange where the company is listed.
    /// </summary>
    public string Exchange { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the International Securities Identification Number (ISIN) of the company.
    /// </summary>
    public string ISIN { get; private set; } = string.Empty;

    /// <summary>
    /// Gets or sets the website of the company (optional).
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Factory method to create a new company with validation.
    /// </summary>
    /// <param name="name">The name of the company.</param>
    /// <param name="ticker">The stock ticker symbol.</param>
    /// <param name="exchange">The stock exchange.</param>
    /// <param name="isin">The ISIN of the company.</param>
    /// <param name="website">The website of the company (optional).</param>
    /// <returns>A <see cref="Result{Company}"/> representing the outcome of the creation.</returns>
    public static Result<Company> Create(string name, string ticker, string exchange, string isin, string? website = null)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(name))
                throw BusinessRuleException.RequiredField("Company", "Name");

            if (string.IsNullOrWhiteSpace(ticker))
                throw BusinessRuleException.RequiredField("Company", "Ticker");

            if (string.IsNullOrWhiteSpace(exchange))
                throw BusinessRuleException.RequiredField("Company", "Exchange");

            if (string.IsNullOrWhiteSpace(isin))
                throw BusinessRuleException.RequiredField("Company", "ISIN");

            // Validate ISIN format using stronger validation
            ValidateIsin(isin);

            // Create new company instance
            var company = new Company
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Ticker = ticker.Trim().ToUpperInvariant(),
                Exchange = exchange.Trim().ToUpperInvariant(),
                ISIN = isin.Trim().ToUpperInvariant(),
                Website = website?.Trim()
            };

            return Result<Company>.Success(company);
        }
        catch (DomainException ex)
        {
            return Result<Company>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Updates the company with new values.
    /// </summary>
    /// <param name="name">The new name of the company.</param>
    /// <param name="ticker">The new stock ticker symbol.</param>
    /// <param name="exchange">The new stock exchange.</param>
    /// <param name="isin">The new ISIN of the company.</param>
    /// <param name="website">The new website of the company (optional).</param>
    /// <returns>A <see cref="Result{Company}"/> representing the outcome of the update.</returns>
    public Result<Company> Update(string name, string ticker, string exchange, string isin, string? website = null)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(name))
                throw BusinessRuleException.RequiredField("Company", "Name");

            if (string.IsNullOrWhiteSpace(ticker))
                throw BusinessRuleException.RequiredField("Company", "Ticker");

            if (string.IsNullOrWhiteSpace(exchange))
                throw BusinessRuleException.RequiredField("Company", "Exchange");

            if (string.IsNullOrWhiteSpace(isin))
                throw BusinessRuleException.RequiredField("Company", "ISIN");

            // Validate ISIN format using stronger validation
            ValidateIsin(isin);

            // Update properties
            Name = name.Trim();
            Ticker = ticker.Trim().ToUpperInvariant();
            Exchange = exchange.Trim().ToUpperInvariant();
            ISIN = isin.Trim().ToUpperInvariant();
            Website = website?.Trim();

            return Result<Company>.Success(this);
        }
        catch (DomainException ex)
        {
            return Result<Company>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Validates ISIN format and throws a <see cref="BusinessRuleException"/> if invalid.
    /// </summary>
    /// <param name="isin">The ISIN to validate.</param>
    private static void ValidateIsin(string isin)
    {
        if (string.IsNullOrWhiteSpace(isin))
            throw BusinessRuleException.RequiredField("Company", "ISIN");

        // Normalize the input
        isin = isin.Trim().ToUpperInvariant();

        // Check exact length (12 characters)
        if (isin.Length != 12)
            throw IsinFormatException.InvalidLength(isin);

        // Check first 2 characters are alphabetic
        if (!CountryCodeRegex.IsMatch(isin))
            throw IsinFormatException.InvalidCountryCode(isin);

        // Check the full pattern
        if (!IsinRegex.IsMatch(isin))
            throw IsinFormatException.InvalidFormat(isin);
    }
}