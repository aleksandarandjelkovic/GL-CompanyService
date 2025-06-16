using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static Company.Tests.Integration.TestHelpers;

namespace Company.Tests.Integration.Companies
{
    public class CompanyApiAuthenticationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly HttpClient _unauthenticatedClient;
        private const string ApiBaseUrl = "/api/companies";

        public CompanyApiAuthenticationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _unauthenticatedClient = fixture.CreateClientWithoutAuthentication();
            
            // Ensure no authentication headers are present
            _unauthenticatedClient.DefaultRequestHeaders.Authorization = null;
        }

        [Fact]
        public async Task GetAll_WithoutAuthentication_Returns401Unauthorized()
        {
            // Act
            var response = await _unauthenticatedClient.GetAsync(ApiBaseUrl);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetById_WithoutAuthentication_Returns401Unauthorized()
        {
            // Act
            var response = await _unauthenticatedClient.GetAsync($"{ApiBaseUrl}/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetByIsin_WithoutAuthentication_Returns401Unauthorized()
        {
            // Act
            var response = await _unauthenticatedClient.GetAsync($"{ApiBaseUrl}/isin/US0378331005");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_WithoutAuthentication_Returns401Unauthorized()
        {
            // Arrange
            var createRequest = CreateCompanyRequestBuilder()
                .WithName("Test Company")
                .WithTicker("TEST")
                .WithExchange("NYSE")
                .WithISIN("US0000000001")
                .Build();

            // Act
            var response = await _unauthenticatedClient.PostAsync(
                ApiBaseUrl,
                CreateJsonContent(createRequest));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Update_WithoutAuthentication_Returns401Unauthorized()
        {
            // Arrange
            var updateRequest = new Companies.Models.UpdateCompanyRequest
            {
                Id = Guid.NewGuid(),
                Name = "Test Company Updated",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "US0000000001"
            };

            // Act
            var response = await _unauthenticatedClient.PutAsync(
                $"{ApiBaseUrl}/{updateRequest.Id}",
                CreateJsonContent(updateRequest));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
} 