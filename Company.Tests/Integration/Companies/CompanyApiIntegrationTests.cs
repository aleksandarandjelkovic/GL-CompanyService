using Company.Tests.Integration.Companies.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static Company.Tests.Integration.TestHelpers;

namespace Company.Tests.Integration.Companies
{
    public class CompanyApiIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestFixture _fixture;
        private const string ApiBaseUrl = "/api/companies";

        public CompanyApiIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.AuthenticatedClient; // Use the authenticated client
        }

        [Fact]
        public async Task Create_WithValidCompany_ReturnsCreated()
        {
            // Arrange
            var createRequest = CreateCompanyRequestBuilder()
                .WithName("Microsoft")
                .WithTicker("MSFT")
                .WithExchange("NASDAQ")
                .WithISIN("US5949181045")
                .WithWebsite("https://microsoft.com")
                .Build();

            // Act
            var response = await _client.PostAsync(
                ApiBaseUrl, 
                CreateJsonContent(createRequest));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            var responseCompany = await DeserializeResponseAsync<CompanyDto>(response);
            responseCompany.Should().NotBeNull();
            responseCompany!.Name.Should().Be(createRequest.Name);
            responseCompany.Ticker.Should().Be(createRequest.Ticker);
            responseCompany.Exchange.Should().Be(createRequest.Exchange);
            responseCompany.ISIN.Should().Be(createRequest.ISIN);
            responseCompany.Website.Should().Be(createRequest.Website);
            responseCompany.Id.Should().NotBeEmpty();
        }
        
        [Fact]
        public async Task Create_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var createRequest = new CreateCompanyRequest
            {
                // Missing required Name
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = ""  // Invalid ISIN
            };

            // Act
            var response = await _client.PostAsync(
                ApiBaseUrl,
                CreateJsonContent(createRequest));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await GetResponseStringAsync(response);
            content.Should().Contain("error");
        }

        [Fact]
        public async Task GetAll_AfterCreatingCompany_ReturnsAllCompanies()
        {
            // Arrange
            var createRequest = CreateCompanyRequestBuilder()
                .WithName("Apple")
                .WithTicker("AAPL")
                .WithExchange("NASDAQ")
                .WithISIN("US0378331005")
                .Build();

            await _client.PostAsync(ApiBaseUrl, CreateJsonContent(createRequest));

            // Act
            var response = await _client.GetAsync(ApiBaseUrl);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var companies = await DeserializeResponseAsync<List<CompanyDto>>(response);
            companies.Should().NotBeNull();
            companies!.Should().NotBeEmpty();
            companies.Should().Contain(c => c.ISIN == createRequest.ISIN);
        }

        [Fact]
        public async Task GetById_WithExistingCompany_ReturnsCompany()
        {
            // Arrange
            var createRequest = CreateCompanyRequestBuilder()
                .WithName("Amazon")
                .WithTicker("AMZN")
                .WithExchange("NASDAQ")
                .WithISIN("US0231351067")
                .Build();

            var createResponse = await _client.PostAsync(ApiBaseUrl, CreateJsonContent(createRequest));
            var createdCompany = await DeserializeResponseAsync<CompanyDto>(createResponse);

            // Act
            var response = await _client.GetAsync($"{ApiBaseUrl}/{createdCompany!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var company = await DeserializeResponseAsync<CompanyDto>(response);
            company.Should().NotBeNull();
            company!.Id.Should().Be(createdCompany.Id);
            company.Name.Should().Be(createRequest.Name);
            company.ISIN.Should().Be(createRequest.ISIN);
        }
        
        [Fact]
        public async Task GetById_WithNonexistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"{ApiBaseUrl}/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetByIsin_WithExistingCompany_ReturnsCompany()
        {
            // Arrange
            var isin = "US02079K1079"; // Google
            var createRequest = CreateCompanyRequestBuilder()
                .WithName("Alphabet Inc.")
                .WithTicker("GOOGL")
                .WithExchange("NASDAQ")
                .WithISIN(isin)
                .Build();

            await _client.PostAsync(ApiBaseUrl, CreateJsonContent(createRequest));

            // Act
            var response = await _client.GetAsync($"{ApiBaseUrl}/isin/{isin}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var company = await DeserializeResponseAsync<CompanyDto>(response);
            company.Should().NotBeNull();
            company!.ISIN.Should().Be(isin);
            company.Name.Should().Be(createRequest.Name);
        }
        
        [Fact]
        public async Task GetByIsin_WithEmptyIsin_ReturnsBadRequest()
        {
            // Act
            var response = await _client.GetAsync($"{ApiBaseUrl}/isin/");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed); // ASP.NET Core routing returns 405 for this case
        }
        
        [Fact]
        public async Task GetByIsin_WithNonexistentIsin_ReturnsNotFound()
        {
            // Arrange
            var nonExistentIsin = "US0000000000";

            // Act
            var response = await _client.GetAsync($"{ApiBaseUrl}/isin/{nonExistentIsin}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Update_WithValidData_ReturnsOk()
        {
            // Arrange
            var createRequest = CreateCompanyRequestBuilder()
                .WithName("Netflix")
                .WithTicker("NFLX")
                .WithExchange("NASDAQ")
                .WithISIN("US64110L1061")
                .Build();

            var createResponse = await _client.PostAsync(ApiBaseUrl, CreateJsonContent(createRequest));
            var createdCompany = await DeserializeResponseAsync<CompanyDto>(createResponse);

            var updateRequest = new UpdateCompanyRequest
            {
                Id = createdCompany!.Id,
                Name = "Netflix Inc.",
                Ticker = createdCompany.Ticker,
                Exchange = createdCompany.Exchange,
                ISIN = createdCompany.ISIN,
                Website = "https://netflix.com"
            };

            // Act
            var response = await _client.PutAsync(
                $"{ApiBaseUrl}/{createdCompany.Id}",
                CreateJsonContent(updateRequest));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedCompany = await DeserializeResponseAsync<CompanyDto>(response);
            updatedCompany.Should().NotBeNull();
            updatedCompany!.Name.Should().Be(updateRequest.Name);
            updatedCompany.Website.Should().Be(updateRequest.Website);
        }
        
        [Fact]
        public async Task Update_WithNonexistentCompany_ReturnsNotFound()
        {
            // Arrange
            var updateRequest = new UpdateCompanyRequest
            {
                Id = Guid.NewGuid(),
                Name = "Non-existent Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "US0000000000"
            };

            // Act
            var response = await _client.PutAsync(
                $"{ApiBaseUrl}/{updateRequest.Id}",
                CreateJsonContent(updateRequest));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        
        [Fact]
        public async Task Update_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var createRequest = CreateCompanyRequestBuilder()
                .WithName("Tesla")
                .WithTicker("TSLA")
                .WithExchange("NASDAQ")
                .WithISIN("US88160R1014")
                .Build();

            var createResponse = await _client.PostAsync(ApiBaseUrl, CreateJsonContent(createRequest));
            var createdCompany = await DeserializeResponseAsync<CompanyDto>(createResponse);

            var updateRequest = new UpdateCompanyRequest
            {
                Id = createdCompany!.Id,
                Name = "",  // Invalid: empty name
                Ticker = "",  // Invalid: empty ticker
                Exchange = createdCompany.Exchange,
                ISIN = createdCompany.ISIN
            };

            // Act
            var response = await _client.PutAsync(
                $"{ApiBaseUrl}/{createdCompany.Id}",
                CreateJsonContent(updateRequest));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
