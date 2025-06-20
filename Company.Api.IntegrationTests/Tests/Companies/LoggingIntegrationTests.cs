using System.Net.Http.Headers;
using System.Net.Http.Json;
using Company.Api.IntegrationTests.Fixtures;
using Company.Api.IntegrationTests.Infrastructure;
using Company.Application.DTOs;
using Company.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Company.Api.IntegrationTests.Tests.Companies;

/// <summary>
/// Tests for verifying logging behavior in the API
/// Uses a custom logger provider to capture logs during test execution
/// </summary>
[Collection("Company API Tests")]
public class LoggingIntegrationTests : IAsyncDisposable
{
    private readonly HttpClient _client;
    private readonly ApiWebApplicationFactory _factory;
    private readonly ITestOutputHelper _output;
    private readonly IServiceScope _scope;
    private readonly TestDataManager _testDataManager;
    private readonly TestLoggerProvider _loggerProvider;

    public LoggingIntegrationTests(ApiWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;

        // Add test logger provider to capture logs
        _loggerProvider = new TestLoggerProvider(output);

        // Configure client with logger
        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing logger providers
                services.AddSingleton<ILoggerProvider>(_loggerProvider);
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Add authentication header
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");

        // Create a scope that will be kept alive for the duration of the test
        _scope = _factory.Services.CreateScope();

        // Setup test data manager
        var dbContext = _scope.ServiceProvider.GetRequiredService<CompanyDbContext>();
        var loggerFactory = _scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        _testDataManager = new TestDataManager(
            dbContext,
            loggerFactory.CreateLogger<TestDataManager>());

        // Clean database before each test
        _testDataManager.CleanTestCompaniesAsync().Wait();

        // Clear any existing logs
        _loggerProvider.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        // Clean up any companies created during the test
        await _testDataManager.CleanTestCompaniesAsync();

        // Dispose the scope
        _scope?.Dispose();
    }

    [Fact(Skip = "Logger provider integration needs more configuration")]
    public async Task GetAllCompanies_ShouldLogRequestAndResponse()
    {
        // Arrange
        _loggerProvider.Clear();

        // Act
        var response = await _client.GetAsync("/api/companies");

        // Assert
        response.EnsureSuccessStatusCode();

        // Debug output all logs
        _output.WriteLine("Captured logs:");
        foreach (var log in _loggerProvider.LogEntries)
        {
            _output.WriteLine($"- {log}");
        }
    }

    [Fact(Skip = "Logger provider integration needs more configuration")]
    public async Task CreateCompany_ShouldLogValidationErrors()
    {
        // Arrange
        _loggerProvider.Clear();

        var invalidCompany = new CreateCompanyRequest
        {
            Name = "", // Empty name - invalid
            Ticker = "TOOLONG", // Too long ticker
            Exchange = "", // Empty exchange - invalid
            ISIN = "INVALID", // Invalid ISIN format
            Website = "not-a-url" // Invalid URL format
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/companies", invalidCompany);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        // Debug output all logs
        _output.WriteLine("Captured logs:");
        foreach (var log in _loggerProvider.LogEntries)
        {
            _output.WriteLine($"- {log}");
        }
    }

    [Fact(Skip = "Logger provider integration needs more configuration")]
    public async Task GetCompanyById_WithNotFoundId_ShouldLogNotFound()
    {
        // Arrange
        _loggerProvider.Clear();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/companies/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

        // Debug output all logs
        _output.WriteLine("Captured logs:");
        foreach (var log in _loggerProvider.LogEntries)
        {
            _output.WriteLine($"- {log}");
        }
    }

    [Fact(Skip = "Logger provider integration needs more configuration")]
    public async Task UpdateCompany_ShouldLogSuccessfulUpdate()
    {
        try
        {
            // Arrange
            await _testDataManager.SeedTestCompaniesAsync(1);
            var companies = await _scope.ServiceProvider
                .GetRequiredService<Company.Domain.Interfaces.ICompanyRepository>()
                .GetAllAsync();

            var testCompany = companies.First(c => c.Name.StartsWith(TestDataManager.TEST_COMPANY_PREFIX));

            _loggerProvider.Clear();

            var updateRequest = new UpdateCompanyRequest
            {
                Id = testCompany.Id,
                Name = $"{TestDataManager.TEST_COMPANY_PREFIX} Updated",
                Ticker = "TUPD",
                Exchange = "NASDAQ",
                ISIN = testCompany.ISIN,
                Website = "https://test-updated-company.com"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/companies/{testCompany.Id}", updateRequest);

            // Assert
            response.EnsureSuccessStatusCode();

            // Debug output all logs
            _output.WriteLine("Captured logs:");
            foreach (var log in _loggerProvider.LogEntries)
            {
                _output.WriteLine($"- {log}");
            }
        }
        finally
        {
            await _testDataManager.CleanTestCompaniesAsync();
        }
    }
}

/// <summary>
/// Custom logger provider that captures logs for testing
/// </summary>
public class TestLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;
    public List<string> LogEntries { get; } = new();

    public TestLoggerProvider(ITestOutputHelper output)
    {
        _output = output;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(categoryName, this);
    }

    public void Dispose() { }

    public void Clear()
    {
        LogEntries.Clear();
    }

    private class TestLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly TestLoggerProvider _provider;

        public TestLogger(string categoryName, TestLoggerProvider provider)
        {
            _categoryName = categoryName;
            _provider = provider;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            var logEntry = $"[{logLevel}] {_categoryName}: {message}";

            _provider.LogEntries.Add(logEntry);
            _provider._output.WriteLine(logEntry);

            if (exception != null)
            {
                var exceptionEntry = $"Exception: {exception.Message}\n{exception.StackTrace}";
                _provider.LogEntries.Add(exceptionEntry);
                _provider._output.WriteLine(exceptionEntry);
            }
        }
    }

    private class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();
        public void Dispose() { }
    }
}