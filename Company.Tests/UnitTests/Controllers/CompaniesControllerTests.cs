using Company.Api.Controllers;
using Company.Application.Common;
using Company.Application.DTOs;
using Company.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Company.Tests.UnitTests.Controllers
{
    public class CompaniesControllerTests
    {
        private readonly Mock<ICompanyService> _mockCompanyService;
        private readonly Mock<ILogger<CompaniesController>> _mockLogger;
        private readonly CompaniesController _controller;

        public CompaniesControllerTests()
        {
            _mockCompanyService = new Mock<ICompanyService>();
            _mockLogger = new Mock<ILogger<CompaniesController>>();
            _controller = new CompaniesController(_mockCompanyService.Object, _mockLogger.Object);
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ReturnsOkWithCompanies()
        {
            // Arrange
            var companies = new List<CompanyResponse>
            {
                new CompanyResponse
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Company",
                    Ticker = "TEST",
                    Exchange = "NYSE",
                    ISIN = "US1234567890"
                }
            };

            _mockCompanyService.Setup(s => s.GetAllCompaniesAsync())
                .ReturnsAsync(companies);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCompanies = Assert.IsAssignableFrom<IEnumerable<CompanyResponse>>(okResult.Value);
            Assert.Equal(companies, returnedCompanies);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_WithExistingId_ReturnsOkWithCompany()
        {
            // Arrange
            var id = Guid.NewGuid();
            var company = new CompanyResponse
            {
                Id = id,
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "US1234567890"
            };

            _mockCompanyService.Setup(s => s.GetCompanyByIdAsync(id))
                .ReturnsAsync(company);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCompany = Assert.IsType<CompanyResponse>(okResult.Value);
            Assert.Equal(company, returnedCompany);
        }

        [Fact]
        public async Task GetById_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockCompanyService.Setup(s => s.GetCompanyByIdAsync(id))
                .ReturnsAsync((CompanyResponse?)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region GetByIsin Tests

        [Fact]
        public async Task GetByIsin_WithExistingIsin_ReturnsOkWithCompany()
        {
            // Arrange
            var isin = "US1234567890";
            var company = new CompanyResponse
            {
                Id = Guid.NewGuid(),
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = isin
            };

            _mockCompanyService.Setup(s => s.GetCompanyByIsinAsync(isin))
                .ReturnsAsync(company);

            // Act
            var result = await _controller.GetByIsin(isin);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCompany = Assert.IsType<CompanyResponse>(okResult.Value);
            Assert.Equal(company, returnedCompany);
        }

        [Fact]
        public async Task GetByIsin_WithEmptyIsin_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetByIsin("");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetByIsin_WithNonExistentIsin_ReturnsNotFound()
        {
            // Arrange
            var isin = "US1234567890";
            _mockCompanyService.Setup(s => s.GetCompanyByIsinAsync(isin))
                .ReturnsAsync((CompanyResponse?)null);

            // Act
            var result = await _controller.GetByIsin(isin);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedWithCompany()
        {
            // Arrange
            var createRequest = new CreateCompanyRequest
            {
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "US1234567890"
            };

            var createdCompany = new CompanyResponse
            {
                Id = Guid.NewGuid(),
                Name = createRequest.Name,
                Ticker = createRequest.Ticker,
                Exchange = createRequest.Exchange,
                ISIN = createRequest.ISIN
            };

            _mockCompanyService.Setup(s => s.CreateCompanyAsync(createRequest))
                .ReturnsAsync(ServiceResult<CompanyResponse>.Success(createdCompany));

            // Act
            var result = await _controller.Create(createRequest);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedCompany = Assert.IsType<CompanyResponse>(createdAtResult.Value);
            Assert.Equal(createdCompany, returnedCompany);
            Assert.Equal(nameof(_controller.GetById), createdAtResult.ActionName);
            Assert.Equal(createdCompany.Id, createdAtResult.RouteValues?["id"]);
        }

        [Fact]
        public async Task Create_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var createRequest = new CreateCompanyRequest
            {
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "INVALID_ISIN"
            };

            _mockCompanyService.Setup(s => s.CreateCompanyAsync(createRequest))
                .ReturnsAsync(ServiceResult<CompanyResponse>.Failure("Invalid ISIN format"));

            // Act
            var result = await _controller.Create(createRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            dynamic value = badRequestResult.Value!;
            Assert.Equal("Invalid ISIN format", value.GetType().GetProperty("error").GetValue(value, null));
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_WithValidData_ReturnsOkWithCompany()
        {
            // Arrange
            var updateRequest = new UpdateCompanyRequest
            {
                Id = Guid.NewGuid(),
                Name = "Updated Company",
                Ticker = "UPDT",
                Exchange = "NASDAQ",
                ISIN = "US9876543210"
            };

            var updatedCompany = new CompanyResponse
            {
                Id = updateRequest.Id,
                Name = updateRequest.Name,
                Ticker = updateRequest.Ticker,
                Exchange = updateRequest.Exchange,
                ISIN = updateRequest.ISIN
            };

            _mockCompanyService.Setup(s => s.UpdateCompanyAsync(updateRequest))
                .ReturnsAsync(ServiceResult<CompanyResponse>.Success(updatedCompany));

            // Act
            var result = await _controller.Update(updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCompany = Assert.IsType<CompanyResponse>(okResult.Value);
            Assert.Equal(updatedCompany, returnedCompany);
        }

        [Fact]
        public async Task Update_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var updateRequest = new UpdateCompanyRequest
            {
                Id = Guid.NewGuid(),
                Name = "Non-Existent Company",
                Ticker = "NONE",
                Exchange = "NYSE",
                ISIN = "US0000000000"
            };

            _mockCompanyService.Setup(s => s.UpdateCompanyAsync(updateRequest))
                .ReturnsAsync(ServiceResult<CompanyResponse>.Failure("Company not found"));

            // Act
            var result = await _controller.Update(updateRequest);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task Update_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var updateRequest = new UpdateCompanyRequest
            {
                Id = Guid.NewGuid(),
                Name = "Updated Company",
                Ticker = "UPDT",
                Exchange = "NASDAQ",
                ISIN = "INVALID_ISIN"
            };

            _mockCompanyService.Setup(s => s.UpdateCompanyAsync(updateRequest))
                .ReturnsAsync(ServiceResult<CompanyResponse>.Failure("Invalid ISIN format"));

            // Act
            var result = await _controller.Update(updateRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        #endregion
    }
} 