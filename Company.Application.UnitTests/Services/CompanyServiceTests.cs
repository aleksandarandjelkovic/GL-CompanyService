using AutoMapper;
using Company.Application.DTOs;
using Company.Application.Mapping;
using Company.Application.Services;
using Company.Domain.Common.Exceptions;
using Company.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Company.Application.UnitTests.Services
{
    public class CompanyServiceTests
    {
        private readonly Mock<ICompanyRepository> _mockRepository;
        private readonly IMapper _mapper;
        private readonly CompanyService _companyService;

        public CompanyServiceTests()
        {
            // Setup AutoMapper with the actual mapping profile
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CompanyMappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            // Setup mock repository
            _mockRepository = new Mock<ICompanyRepository>();

            // Create the service with mocked dependencies
            _companyService = new CompanyService(_mockRepository.Object, _mapper);
        }

        #region GetAllCompaniesAsync Tests

        [Fact]
        public async Task GetAllCompaniesAsync_ShouldReturnAllCompanies()
        {
            // Arrange
            var companies = new List<Domain.Entities.Company>
            {
                CreateTestCompany("Company 1", "TKR1", "NYSE", "US0000000001"),
                CreateTestCompany("Company 2", "TKR2", "NASDAQ", "US0000000002"),
                CreateTestCompany("Company 3", "TKR3", "LSE", "GB0000000003")
            };

            _mockRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(companies);

            // Act
            var result = await _companyService.GetAllCompaniesAsync();

            // Assert
            var resultList = result.ToList();
            resultList.Should().HaveCount(companies.Count);

            for (int i = 0; i < companies.Count; i++)
            {
                resultList[i].Name.Should().Be(companies[i].Name);
                resultList[i].Ticker.Should().Be(companies[i].Ticker);
                resultList[i].ISIN.Should().Be(companies[i].ISIN);
            }

            // Verify repository was called
            _mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        #endregion

        #region GetCompanyByIdAsync Tests

        [Fact]
        public async Task GetCompanyByIdAsync_ShouldReturnCorrectCompanyDTO()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var company = CreateTestCompany(
                "Test Company",
                "TEST",
                "NYSE",
                "US1234567890",
                "https://test-company.com"
            );

            // Use reflection to set the ID (since Id is typically read-only)
            typeof(Domain.Entities.Company).GetProperty("Id")!.SetValue(company, companyId);

            _mockRepository.Setup(repo => repo.GetByIdAsync(companyId))
                .ReturnsAsync(company);

            // Act
            var result = await _companyService.GetCompanyByIdAsync(companyId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(companyId);
            result.Name.Should().Be(company.Name);
            result.Ticker.Should().Be(company.Ticker);
            result.Exchange.Should().Be(company.Exchange);
            result.ISIN.Should().Be(company.ISIN);
            result.Website.Should().Be(company.Website);

            // Verify repository was called with correct ID
            _mockRepository.Verify(repo => repo.GetByIdAsync(companyId), Times.Once);
        }

        [Fact]
        public async Task GetCompanyByIdAsync_WhenCompanyDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            _mockRepository.Setup(repo => repo.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Domain.Entities.Company)null!);

            // Act
            var result = await _companyService.GetCompanyByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();

            // Verify repository was called with correct ID
            _mockRepository.Verify(repo => repo.GetByIdAsync(nonExistentId), Times.Once);
        }

        #endregion

        #region GetCompanyByIsinAsync Tests

        [Fact]
        public async Task GetCompanyByIsinAsync_ShouldReturnCorrectCompany()
        {
            // Arrange
            var isin = "US1234567890";
            var company = CreateTestCompany("Test Company", "TEST", "NYSE", isin);

            _mockRepository.Setup(repo => repo.GetByIsinAsync(isin.ToUpperInvariant()))
                .ReturnsAsync(company);

            // Act
            var result = await _companyService.GetCompanyByIsinAsync(isin);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be(company.Name);
            result.Ticker.Should().Be(company.Ticker);
            result.ISIN.Should().Be(company.ISIN);

            // Verify repository was called with normalized ISIN
            _mockRepository.Verify(repo => repo.GetByIsinAsync(isin.ToUpperInvariant()), Times.Once);
        }

        [Fact]
        public async Task GetCompanyByIsinAsync_ShouldNormalizeIsin()
        {
            // Arrange
            var originalIsin = "  us1234567890  "; // lowercase with spaces
            var normalizedIsin = "US1234567890";   // uppercase, trimmed
            var company = CreateTestCompany("Test Company", "TEST", "NYSE", normalizedIsin);

            _mockRepository.Setup(repo => repo.GetByIsinAsync(normalizedIsin))
                .ReturnsAsync(company);

            // Act
            var result = await _companyService.GetCompanyByIsinAsync(originalIsin);

            // Assert
            result.Should().NotBeNull();

            // Verify repository was called with normalized ISIN
            _mockRepository.Verify(repo => repo.GetByIsinAsync(normalizedIsin), Times.Once);
        }

        [Fact]
        public async Task GetCompanyByIsinAsync_WhenCompanyDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var nonExistentIsin = "US0000000000";

            _mockRepository.Setup(repo => repo.GetByIsinAsync(nonExistentIsin))
                .ReturnsAsync((Domain.Entities.Company)null!);

            // Act
            var result = await _companyService.GetCompanyByIsinAsync(nonExistentIsin);

            // Assert
            result.Should().BeNull();

            // Verify repository was called with correct ISIN
            _mockRepository.Verify(repo => repo.GetByIsinAsync(nonExistentIsin), Times.Once);
        }

        #endregion

        #region CreateCompanyAsync Tests

        [Fact]
        public async Task CreateCompanyAsync_ShouldAddCompanyAndReturnSuccess()
        {
            // Arrange
            var companyRequest = new CreateCompanyRequest
            {
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "US1234567890",
                Website = "https://test-company.com"
            };

            // Setup repo to return that the ISIN is unique
            _mockRepository.Setup(repo => repo.IsIsinUniqueAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Setup repo to properly add the company
            _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Domain.Entities.Company>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _companyService.CreateCompanyAsync(companyRequest);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().NotBe(Guid.Empty);
            result.Value.Name.Should().Be(companyRequest.Name);
            result.Value.ISIN.Should().Be(companyRequest.ISIN.ToUpperInvariant());

            // Verify the repository was called to check uniqueness and add the company
            _mockRepository.Verify(repo => repo.IsIsinUniqueAsync(It.Is<string>(isin =>
                isin.Equals(companyRequest.ISIN, StringComparison.OrdinalIgnoreCase))), Times.Once);
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Company>()), Times.Once);
        }

        [Fact]
        public async Task CreateCompanyAsync_WithDuplicateISIN_ShouldThrowBusinessRuleException()
        {
            // Arrange
            var companyRequest = new CreateCompanyRequest
            {
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "US1234567890",
                Website = "https://test-company.com"
            };

            // Setup repo to return that the ISIN is not unique
            _mockRepository.Setup(repo => repo.IsIsinUniqueAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _companyService.CreateCompanyAsync(companyRequest));

            // Verify the repository was called to check uniqueness but not to add the company
            _mockRepository.Verify(repo => repo.IsIsinUniqueAsync(It.Is<string>(isin =>
                isin.Equals(companyRequest.ISIN, StringComparison.OrdinalIgnoreCase))), Times.Once);
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Company>()), Times.Never);
        }

        [Fact]
        public async Task CreateCompanyAsync_WithInvalidISINFormat_ShouldReturnFailure()
        {
            // Arrange
            var companyRequest = new CreateCompanyRequest
            {
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "INVALID", // Invalid ISIN format
                Website = "https://test-company.com"
            };

            // Act
            var result = await _companyService.CreateCompanyAsync(companyRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
            result.Value.Should().BeNull();

            // Verify the repository was not called
            _mockRepository.Verify(repo => repo.IsIsinUniqueAsync(It.IsAny<string>()), Times.Never);
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Company>()), Times.Never);
        }

        #endregion

        #region UpdateCompanyAsync Tests

        [Fact]
        public async Task UpdateCompanyAsync_ShouldUpdateAndReturnCompany()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var existingCompany = CreateTestCompany(
                "Original Company",
                "ORG",
                "NYSE",
                "US1234567890",
                "https://original-company.com"
            );

            // Use reflection to set the ID
            typeof(Domain.Entities.Company).GetProperty("Id")!.SetValue(existingCompany, companyId);

            var updateRequest = new UpdateCompanyRequest
            {
                Id = companyId,
                Name = "Updated Company",
                Ticker = "UPD",
                Exchange = "NASDAQ",
                ISIN = "US1234567890", // Same ISIN
                Website = "https://updated-company.com"
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(companyId))
                .ReturnsAsync(existingCompany);

            _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Company>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _companyService.UpdateCompanyAsync(updateRequest);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(companyId);
            result.Value.Name.Should().Be(updateRequest.Name);
            result.Value.Ticker.Should().Be(updateRequest.Ticker);
            result.Value.Exchange.Should().Be(updateRequest.Exchange);
            result.Value.Website.Should().Be(updateRequest.Website);

            // Verify repository methods were called
            _mockRepository.Verify(repo => repo.GetByIdAsync(companyId), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Company>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCompanyAsync_WithNonExistentId_ShouldThrowEntityNotFoundException()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var updateRequest = new UpdateCompanyRequest
            {
                Id = companyId,
                Name = "Updated Company",
                Ticker = "UPD",
                Exchange = "NASDAQ",
                ISIN = "US1234567890",
                Website = "https://updated-company.com"
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(companyId))
                .ReturnsAsync((Domain.Entities.Company)null!);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _companyService.UpdateCompanyAsync(updateRequest));

            // Verify repository methods were called
            _mockRepository.Verify(repo => repo.GetByIdAsync(companyId), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Company>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCompanyAsync_WithDuplicateISIN_ShouldThrowBusinessRuleException()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var existingCompany = CreateTestCompany(
                "Original Company",
                "ORG",
                "NYSE",
                "US1234567890"
            );

            // Use reflection to set the ID
            typeof(Domain.Entities.Company).GetProperty("Id")!.SetValue(existingCompany, companyId);

            var updateRequest = new UpdateCompanyRequest
            {
                Id = companyId,
                Name = "Updated Company",
                Ticker = "UPD",
                Exchange = "NASDAQ",
                ISIN = "US9876543210", // Different ISIN
                Website = "https://updated-company.com"
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(companyId))
                .ReturnsAsync(existingCompany);

            // Setup repo to return that the new ISIN is not unique
            _mockRepository.Setup(repo => repo.IsIsinUniqueAsync(updateRequest.ISIN))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _companyService.UpdateCompanyAsync(updateRequest));

            // Verify repository methods were called
            _mockRepository.Verify(repo => repo.GetByIdAsync(companyId), Times.Once);
            _mockRepository.Verify(repo => repo.IsIsinUniqueAsync(updateRequest.ISIN), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Company>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCompanyAsync_WithInvalidData_ShouldReturnFailure()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var existingCompany = CreateTestCompany(
                "Original Company",
                "ORG",
                "NYSE",
                "US1234567890"
            );

            // Use reflection to set the ID
            typeof(Domain.Entities.Company).GetProperty("Id")!.SetValue(existingCompany, companyId);

            var updateRequest = new UpdateCompanyRequest
            {
                Id = companyId,
                Name = "", // Invalid empty name
                Ticker = "UPD",
                Exchange = "NASDAQ",
                ISIN = "US1234567890",
                Website = "https://updated-company.com"
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(companyId))
                .ReturnsAsync(existingCompany);

            // Act
            var result = await _companyService.UpdateCompanyAsync(updateRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
            result.Value.Should().BeNull();

            // Verify repository methods were called
            _mockRepository.Verify(repo => repo.GetByIdAsync(companyId), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Company>()), Times.Never);
        }

        #endregion

        // Helper method to create a test company (moved from TestUtils)
        private Domain.Entities.Company CreateTestCompany(string name, string ticker, string exchange, string isin, string? website = null)
        {
            var result = Domain.Entities.Company.Create(name, ticker, exchange, isin, website);
            if (result.IsSuccess)
                return result.Value!;

            throw new InvalidOperationException($"Failed to create test company: {result.Error}");
        }
    }
}
