using Company.Api.Controllers;
using Company.Application.Common;
using Company.Application.DTOs;
using Company.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Company.Api.UnitTests.Controllers;

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
    public async Task GetAll_ShouldReturnOkResult_WithListOfCompanies()
    {
        // Arrange
        var companies = new List<CompanyResponse>
        {
            new CompanyResponse { Id = Guid.NewGuid(), Name = "Test Company 1", Ticker = "TC1", Exchange = "NYSE", ISIN = "US0000000001" },
            new CompanyResponse { Id = Guid.NewGuid(), Name = "Test Company 2", Ticker = "TC2", Exchange = "NASDAQ", ISIN = "US0000000002" }
        };

        _mockCompanyService
            .Setup(s => s.GetAllCompaniesAsync())
            .ReturnsAsync(companies);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCompanies = Assert.IsAssignableFrom<IEnumerable<CompanyResponse>>(okResult.Value);
        Assert.Equal(2, returnedCompanies.Count());
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ShouldReturnOkResult_WhenCompanyExists()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = new CompanyResponse
        {
            Id = companyId,
            Name = "Test Company",
            Ticker = "TEST",
            Exchange = "NYSE",
            ISIN = "US0000000001"
        };

        _mockCompanyService
            .Setup(s => s.GetCompanyByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        var result = await _controller.GetById(companyId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCompany = Assert.IsType<CompanyResponse>(okResult.Value);
        Assert.Equal(companyId, returnedCompany.Id);
        Assert.Equal("Test Company", returnedCompany.Name);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenCompanyDoesNotExist()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        _mockCompanyService
            .Setup(s => s.GetCompanyByIdAsync(companyId))
            .ReturnsAsync((CompanyResponse)null!);

        // Act
        var result = await _controller.GetById(companyId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    #endregion

    #region GetByIsin Tests

    [Fact]
    public async Task GetByIsin_ShouldReturnOkResult_WhenCompanyExists()
    {
        // Arrange
        var isin = "US0000000001";
        var company = new CompanyResponse
        {
            Id = Guid.NewGuid(),
            Name = "Test Company",
            Ticker = "TEST",
            Exchange = "NYSE",
            ISIN = isin
        };

        _mockCompanyService
            .Setup(s => s.GetCompanyByIsinAsync(isin))
            .ReturnsAsync(company);

        // Act
        var result = await _controller.GetByIsin(isin);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCompany = Assert.IsType<CompanyResponse>(okResult.Value);
        Assert.Equal(isin, returnedCompany.ISIN);
        Assert.Equal("Test Company", returnedCompany.Name);
    }

    [Fact]
    public async Task GetByIsin_ShouldReturnBadRequest_WhenIsinIsEmpty()
    {
        // Act
        var result = await _controller.GetByIsin("");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByIsin_ShouldReturnNotFound_WhenCompanyDoesNotExist()
    {
        // Arrange
        var isin = "US0000000001";

        _mockCompanyService
            .Setup(s => s.GetCompanyByIsinAsync(isin))
            .ReturnsAsync((CompanyResponse)null!);

        // Act
        var result = await _controller.GetByIsin(isin);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenCompanyIsCreated()
    {
        // Arrange
        var request = new CreateCompanyRequest
        {
            Name = "New Company",
            Ticker = "NEW",
            Exchange = "NYSE",
            ISIN = "US0000000003"
        };

        var createdCompany = new CompanyResponse
        {
            Id = Guid.NewGuid(),
            Name = "New Company",
            Ticker = "NEW",
            Exchange = "NYSE",
            ISIN = "US0000000003"
        };

        _mockCompanyService
            .Setup(s => s.CreateCompanyAsync(It.IsAny<CreateCompanyRequest>()))
            .ReturnsAsync(ServiceResult<CompanyResponse>.Success(createdCompany));

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedCompany = Assert.IsType<CompanyResponse>(createdAtActionResult.Value!);
        Assert.Equal("New Company", returnedCompany.Name);
        Assert.Equal("NEW", returnedCompany.Ticker);
        Assert.Equal(nameof(_controller.GetById), createdAtActionResult.ActionName);
        Assert.Equal(createdCompany.Id, createdAtActionResult.RouteValues!["id"]);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenModelIsInvalid()
    {
        // Arrange
        var request = new CreateCompanyRequest
        {
            Name = "New Company",
            Ticker = "NEW",
            Exchange = "NYSE",
            ISIN = "INVALID" // Invalid ISIN
        };

        _mockCompanyService
            .Setup(s => s.CreateCompanyAsync(It.IsAny<CreateCompanyRequest>()))
            .ReturnsAsync(ServiceResult<CompanyResponse>.Failure("Invalid ISIN format"));

        // Act
        var result = await _controller.Create(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        dynamic errorResponse = badRequestResult.Value!;
        Assert.Equal("Invalid ISIN format", errorResponse.error);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldReturnOkResult_WhenCompanyIsUpdated()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var request = new UpdateCompanyRequest
        {
            Id = companyId,
            Name = "Updated Company",
            Ticker = "UPD",
            Exchange = "NYSE",
            ISIN = "US0000000004"
        };

        var updatedCompany = new CompanyResponse
        {
            Id = companyId,
            Name = "Updated Company",
            Ticker = "UPD",
            Exchange = "NYSE",
            ISIN = "US0000000004"
        };

        _mockCompanyService
            .Setup(s => s.UpdateCompanyAsync(It.IsAny<UpdateCompanyRequest>()))
            .ReturnsAsync(ServiceResult<CompanyResponse>.Success(updatedCompany));

        // Act
        var result = await _controller.Update(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCompany = Assert.IsType<CompanyResponse>(okResult.Value!);
        Assert.Equal("Updated Company", returnedCompany.Name);
        Assert.Equal("UPD", returnedCompany.Ticker);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenIdMismatch()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        var request = new UpdateCompanyRequest
        {
            Id = differentId,
            Name = "Updated Company",
            Ticker = "UPD",
            Exchange = "NYSE",
            ISIN = "US0000000004"
        };

        // Act
        var result = await _controller.Update(companyId, request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        dynamic errorResponse = badRequestResult.Value!;
        Assert.Contains("ID mismatch", errorResponse.error.ToString());
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenCompanyDoesNotExist()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var request = new UpdateCompanyRequest
        {
            Id = companyId,
            Name = "Updated Company",
            Ticker = "UPD",
            Exchange = "NYSE",
            ISIN = "US0000000004"
        };

        _mockCompanyService
            .Setup(s => s.UpdateCompanyAsync(It.IsAny<UpdateCompanyRequest>()))
            .ThrowsAsync(new Domain.Common.Exceptions.EntityNotFoundException("Company", companyId));

        // Act
        var result = await _controller.Update(companyId, request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenModelIsInvalid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var request = new UpdateCompanyRequest
        {
            Id = companyId,
            Name = "Updated Company",
            Ticker = "UPD",
            Exchange = "NYSE",
            ISIN = "INVALID" // Invalid ISIN
        };

        _mockCompanyService
            .Setup(s => s.UpdateCompanyAsync(It.IsAny<UpdateCompanyRequest>()))
            .ReturnsAsync(ServiceResult<CompanyResponse>.Failure("Invalid ISIN format"));

        // Act
        var result = await _controller.Update(companyId, request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        dynamic errorResponse = badRequestResult.Value!;
        Assert.Equal("Invalid ISIN format", errorResponse.error);
    }

    #endregion
}