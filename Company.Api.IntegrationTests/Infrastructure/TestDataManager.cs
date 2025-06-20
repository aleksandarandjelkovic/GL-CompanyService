using Company.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Company.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Manages test data for integration tests
/// </summary>
public class TestDataManager(CompanyDbContext dbContext, ILogger<TestDataManager> logger)
{
    private readonly CompanyDbContext _dbContext = dbContext;
    private readonly ILogger<TestDataManager> _logger = logger;

    // Use the constant from TestDataFactory
    public static string TEST_COMPANY_PREFIX => TestDataFactory.TEST_COMPANY_PREFIX;

    /// <summary>
    /// Cleans all test companies from the database
    /// </summary>
    public async Task CleanTestCompaniesAsync()
    {
        try
        {
            // Use parameterized SQL to avoid SQL injection
            string sql = "DELETE FROM \"Companies\" WHERE \"Name\" LIKE @prefix";
            var parameter = new NpgsqlParameter("@prefix", $"{TEST_COMPANY_PREFIX}%");
            await _dbContext.Database.ExecuteSqlRawAsync(sql, parameter);

            _logger.LogInformation("Database cleaned - removed all test companies");

            // Log the remaining companies (should be the seeded ones)
            var remainingCompanies = await _dbContext.Companies.ToListAsync();
            _logger.LogInformation("Database contains {Count} companies after cleanup", remainingCompanies.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning test companies: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Seeds the database with test companies
    /// </summary>
    /// <param name="count">Number of test companies to create</param>
    /// <param name="customSetup">Optional action to customize companies before saving</param>
    /// <returns>Task representing the async operation</returns>
    public async Task SeedTestCompaniesAsync(int count = 3, Action<List<Domain.Entities.Company>>? customSetup = null)
    {
        try
        {
            // First clean any existing test data
            await CleanTestCompaniesAsync();

            // Create test companies using the factory
            var companies = TestDataFactory.CreateTestCompanies(count);

            // Allow for custom setup if provided
            customSetup?.Invoke(companies);

            // Add companies to database
            await _dbContext.Companies.AddRangeAsync(companies);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} test companies", companies.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding test companies: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Seeds a single test company with specific values
    /// </summary>
    /// <returns>The created company entity</returns>
    public async Task<Domain.Entities.Company> SeedSingleTestCompanyAsync(
        string nameSuffix = "",
        string? ticker = null,
        string exchange = "NYSE",
        string? isin = null,
        string? website = null)
    {
        try
        {
            var company = TestDataFactory.CreateTestCompany(nameSuffix, ticker, exchange, isin, website);

            await _dbContext.Companies.AddAsync(company);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Seeded single test company: {CompanyName}", company.Name);
            return company;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding single test company: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Verifies that only the expected number of seeded companies exist in the database
    /// </summary>
    public async Task<bool> VerifyOnlySeededCompaniesExistAsync(int expectedCount = 5)
    {
        var companies = await _dbContext.Companies.ToListAsync();
        var testCompanies = companies.Where(c => c.Name.StartsWith(TEST_COMPANY_PREFIX)).ToList();

        if (testCompanies.Count > 0)
        {
            _logger.LogWarning("Found {Count} test companies that should have been cleaned up", testCompanies.Count);
            return false;
        }

        if (companies.Count != expectedCount)
        {
            _logger.LogWarning("Expected {Expected} seeded companies but found {Actual}", expectedCount, companies.Count);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets all test companies from the database
    /// </summary>
    public async Task<List<Domain.Entities.Company>> GetTestCompaniesAsync()
    {
        return await _dbContext.Companies
            .Where(c => c.Name.StartsWith(TEST_COMPANY_PREFIX))
            .ToListAsync();
    }
}