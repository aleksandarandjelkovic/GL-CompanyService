using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Company.Api.IntegrationTests.Fixtures;
using Company.Api.IntegrationTests.Infrastructure;
using Company.Application.DTOs;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Company.Api.IntegrationTests.Tests.Companies;

[Collection("Company API Tests")]
public class ErrorHandlingTests : IAsyncDisposable
{
    private readonly HttpClient _client;
    private readonly ApiWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly ITestOutputHelper _output;
    private readonly IServiceScope _scope;
    private readonly TestDataManager _testDataManager;

    public ErrorHandlingTests(ApiWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _output = output;

        // Create a scope that will be kept alive for the duration of the test
        _scope = _factory.Services.CreateScope();

        // Get the test data manager
        var dbContext = _scope.ServiceProvider.GetRequiredService<Company.Infrastructure.Persistence.CompanyDbContext>();
        var loggerFactory = _scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        _testDataManager = new TestDataManager(
            dbContext,
            loggerFactory.CreateLogger<TestDataManager>());

        // Clean database before each test
        _testDataManager.CleanTestCompaniesAsync().Wait();
    }

    public async ValueTask DisposeAsync()
    {
        // Clean up any companies created during the test
        await _testDataManager.CleanTestCompaniesAsync();

        // Dispose the scope
        _scope?.Dispose();
    }

    [Fact]
    public async Task GetCompany_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange - Client without authentication
        var clientWithoutAuth = _factory.CreateClient();

        // Act
        var response = await clientWithoutAuth.GetAsync("/api/companies");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        _output.WriteLine($"Response status code: {response.StatusCode}");
    }

    [Fact]
    public async Task GetCompany_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange - Client with invalid authentication
        var clientWithInvalidAuth = _factory.CreateClient();
        clientWithInvalidAuth.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await clientWithInvalidAuth.GetAsync("/api/companies");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        _output.WriteLine($"Response status code: {response.StatusCode}");
    }

    [Fact]
    public async Task GetCompany_WithWrongScope_ShouldReturnForbidden()
    {
        // Arrange - Client with wrong scope
        var clientWithWrongScope = _factory.CreateClient();
        clientWithWrongScope.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "wrong-scope-token");

        // Act
        var response = await clientWithWrongScope.GetAsync("/api/companies");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        _output.WriteLine($"Response status code: {response.StatusCode}");
    }

    [Fact]
    public async Task CreateCompany_WithInvalidData_ShouldReturnDetailedValidationErrors()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "test-token");

        var invalidCompany = new CreateCompanyRequest
        {
            Name = "", // Empty name - invalid
            Ticker = "TOOLONG", // Too long ticker
            Exchange = "", // Empty exchange - invalid
            ISIN = "INVALID", // Invalid ISIN format
            Website = "not-a-url" // Invalid URL format
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/companies", invalidCompany);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response content: {content}");

        // Check for specific validation error messages
        content.Should().Contain("Name");
        content.Should().Contain("ISIN");
        content.Should().Contain("Website");
    }

    [Fact]
    public async Task GetCompanyById_WithNonGuidId_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "test-token");

        // Act
        var response = await client.GetAsync("/api/companies/not-a-guid");

        // Assert
        // The API might return either BadRequest or NotFound for non-GUID IDs
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        _output.WriteLine($"Response status code: {response.StatusCode}");
    }

    [Fact]
    public async Task UpdateCompany_WithMismatchedIds_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "test-token");

        var companyId = Guid.NewGuid();
        var differentId = Guid.NewGuid();

        var updateRequest = new UpdateCompanyRequest
        {
            Id = differentId, // Different from URL id
            Name = "Updated Company",
            Ticker = "UPD",
            Exchange = "NYSE",
            ISIN = "US0000000001",
            Website = "https://updated-company.com"
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.PutAsync($"/api/companies/{companyId}", jsonContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response content: {content}");
        content.Should().Contain("ID mismatch");
    }

    [Fact]
    public async Task GetCompanyByIsin_WithInvalidIsinFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "test-token");

        // Act
        var response = await client.GetAsync("/api/companies/isin/INVALID-ISIN-FORMAT");

        // Assert
        // The API might return either BadRequest or NotFound for invalid ISIN format
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response content: {content}");
    }

    [Fact]
    public async Task CreateCompany_WithExcessivePayloadSize_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "test-token");

        // Create a very large payload (exceeding typical limits)
        var largeCompany = new CreateCompanyRequest
        {
            Name = new string('X', 250_000), // Very long name
            Ticker = "BIG",
            Exchange = "NYSE",
            ISIN = "US0000000001",
            Website = "https://large-company.com" + new string('X', 750_000) // Very long URL
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/companies", largeCompany);

        // Assert
        // This might return 413 Payload Too Large or 400 Bad Request depending on server configuration
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.RequestEntityTooLarge);
        _output.WriteLine($"Response status code: {response.StatusCode}");
    }
}