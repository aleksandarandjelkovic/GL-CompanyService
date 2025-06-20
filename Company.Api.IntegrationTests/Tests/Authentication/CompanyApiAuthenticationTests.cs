using System.Net;
using System.Net.Http.Headers;
using Company.Api.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Company.Api.IntegrationTests.Tests.Authentication
{
    public class CompanyApiAuthenticationTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ApiWebApplicationFactory _factory;

        public CompanyApiAuthenticationTests(ApiWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task Api_ShouldRequireAuthentication()
        {
            // Act - Call API without authentication
            var response = await _client.GetAsync("/api/companies");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Api_ShouldAllowAccess_WithValidToken()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");

            // Act
            var response = await _client.GetAsync("/api/companies");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Api_ShouldDenyAccess_WithInvalidToken()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

            // Act
            var response = await _client.GetAsync("/api/companies");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Api_ShouldDenyAccess_WithExpiredToken()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "expired-token");

            // Act
            var response = await _client.GetAsync("/api/companies");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Api_ShouldRequireCorrectScope()
        {
            // Arrange - Token with wrong scope
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "wrong-scope-token");

            // Act
            var response = await _client.GetAsync("/api/companies");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}