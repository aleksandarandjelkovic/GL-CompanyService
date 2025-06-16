using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;

namespace Company.Tests.Integration.Authentication
{
    public class AuthenticationIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly HttpClient _client;
        private readonly HttpClient _authenticatedClient;
        private const string ApiBaseUrl = "/api/companies";

        public AuthenticationIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client; // Unauthenticated client
            _authenticatedClient = fixture.AuthenticatedClient; // Authenticated client with token
        }

        [Fact]
        public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync(ApiBaseUrl);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetById_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync($"{ApiBaseUrl}/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetByIsin_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync($"{ApiBaseUrl}/isin/US0378331005");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.PostAsync(
                ApiBaseUrl,
                new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Update_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.PutAsync(
                $"{ApiBaseUrl}/{Guid.NewGuid()}",
                new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAll_WithAuthentication_ReturnsOk()
        {
            // Act
            var response = await _authenticatedClient.GetAsync(ApiBaseUrl);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Create_WithAuthentication_AcceptsRequest()
        {
            // Arrange
            var createRequest = TestHelpers.CreateCompanyRequestBuilder()
                .WithName("Auth Test Company")
                .WithTicker("AUTH")
                .WithExchange("TEST")
                .WithISIN("US0000000001")
                .Build();

            // Act
            var response = await _authenticatedClient.PostAsync(
                ApiBaseUrl,
                TestHelpers.CreateJsonContent(createRequest));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }
} 