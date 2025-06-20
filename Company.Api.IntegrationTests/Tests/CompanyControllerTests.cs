using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Company.Api.IntegrationTests.Fixtures;
using Company.Api.IntegrationTests.Tests.Companies.Models;
using FluentAssertions;

namespace Company.Api.IntegrationTests.Tests;

public class CompanyControllerTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ApiWebApplicationFactory _factory;

    public CompanyControllerTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Add authentication token for all requests
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
    }

    [Fact]
    public async Task Get_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/companies");

        // Assert
        response.EnsureSuccessStatusCode(); // Status code 200-299
        var companies = await response.Content.ReadFromJsonAsync<IEnumerable<CompanyDto>>();
        companies.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid(); // Assuming this GUID doesn't exist

        // Act
        var response = await _client.GetAsync($"/api/companies/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}