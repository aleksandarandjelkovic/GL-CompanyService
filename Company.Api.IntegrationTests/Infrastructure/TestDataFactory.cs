namespace Company.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Factory for creating test data consistently across all test projects
/// </summary>
public static class TestDataFactory
{
    /// <summary>
    /// Prefix for all test companies to identify them for cleanup
    /// </summary>
    public const string TEST_COMPANY_PREFIX = "Test Company";

    /// <summary>
    /// Creates a test company with standard or custom values
    /// </summary>
    /// <param name="nameSuffix">Optional suffix to append to the company name</param>
    /// <param name="ticker">Optional ticker symbol (defaults to "TEST" + first 3 chars of nameSuffix)</param>
    /// <param name="exchange">Optional exchange name (defaults to "NYSE")</param>
    /// <param name="isin">Optional ISIN (generates a unique one if not provided)</param>
    /// <param name="website">Optional website URL</param>
    /// <returns>A company entity if creation is successful, throws exception otherwise</returns>
    public static Domain.Entities.Company CreateTestCompany(
        string nameSuffix = "",
        string? ticker = null,
        string exchange = "NYSE",
        string? isin = null,
        string? website = null)
    {
        // Generate default values if not provided
        var name = string.IsNullOrEmpty(nameSuffix)
            ? $"{TEST_COMPANY_PREFIX} {Guid.NewGuid().ToString("N").Substring(0, 8)}"
            : $"{TEST_COMPANY_PREFIX} {nameSuffix}";

        ticker ??= "T" + (string.IsNullOrEmpty(nameSuffix)
            ? "EST"
            : nameSuffix.Substring(0, Math.Min(3, nameSuffix.Length)).ToUpperInvariant());

        isin ??= GenerateValidIsin();

        // Create the company
        var result = Domain.Entities.Company.Create(name, ticker, exchange, isin, website);

        if (result.IsSuccess && result.Value != null)
            return result.Value;

        throw new InvalidOperationException($"Failed to create test company: {result.Error}");
    }

    /// <summary>
    /// Creates a batch of test companies with sequential numbering
    /// </summary>
    /// <param name="count">Number of companies to create</param>
    /// <returns>List of company entities</returns>
    public static List<Domain.Entities.Company> CreateTestCompanies(int count)
    {
        var companies = new List<Domain.Entities.Company>();

        for (int i = 1; i <= count; i++)
        {
            var suffix = i.ToString("D3"); // 001, 002, etc.
            companies.Add(CreateTestCompany(
                nameSuffix: suffix,
                ticker: $"T{suffix}",
                isin: GenerateValidIsin(suffix)
            ));
        }

        return companies;
    }

    /// <summary>
    /// Creates a company with specific values for testing edge cases
    /// </summary>
    public static Domain.Entities.Company CreateCompanyWithSpecificValues(
        string name,
        string ticker,
        string exchange,
        string isin,
        string? website = null)
    {
        var result = Domain.Entities.Company.Create(name, ticker, exchange, isin, website);

        if (result.IsSuccess && result.Value != null)
            return result.Value;

        throw new InvalidOperationException($"Failed to create company with specific values: {result.Error}");
    }

    /// <summary>
    /// Generates a valid ISIN for test companies
    /// </summary>
    /// <param name="suffix">Optional suffix to make the ISIN unique</param>
    /// <returns>A valid ISIN string</returns>
    public static string GenerateValidIsin(string? suffix = null)
    {
        // ISIN format: 2 alpha (country code) + 9 alphanumeric + 1 numeric (check digit)
        // We'll use US as the country code, then a timestamp + random part for uniqueness

        // Get the current timestamp in ticks + random GUID to ensure uniqueness
        var timestamp = DateTime.UtcNow.Ticks.ToString();
        var random = Guid.NewGuid().ToString("N").Substring(0, 4);

        // Create a unique identifier using the suffix, timestamp, and a random part
        var uniqueId = $"{suffix ?? ""}{timestamp}{random}";

        // Take only alphanumeric characters and ensure we have exactly 9 characters for the middle part
        var alphaNumeric = new string(uniqueId.Where(char.IsLetterOrDigit).ToArray());
        var middlePart = alphaNumeric.Substring(0, Math.Min(9, alphaNumeric.Length)).PadRight(9, '0');

        // Use '0' as the check digit for simplicity
        return $"US{middlePart}0";
    }
}