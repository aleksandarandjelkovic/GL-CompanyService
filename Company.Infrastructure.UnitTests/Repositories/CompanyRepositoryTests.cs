using Company.Infrastructure.Persistence;
using Company.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Company.Infrastructure.UnitTests.Repositories;

public class CompanyRepositoryTests
{
    private DbContextOptions<CompanyDbContext> GetDbContextOptions()
    {
        return new DbContextOptionsBuilder<CompanyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private Domain.Entities.Company CreateTestCompany(string name, string ticker = "TEST", string exchange = "NYSE", string isin = "US0000000001", string? website = null)
    {
        var result = Domain.Entities.Company.Create(name, ticker, exchange, isin, website);
        if (result.IsSuccess)
            return result.Value!;

        throw new InvalidOperationException($"Failed to create test company: {result.Error}");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCompanies()
    {
        // Arrange
        var options = GetDbContextOptions();
        using (var context = new CompanyDbContext(options))
        {
            context.Companies.AddRange(
                CreateTestCompany("Test Company 1", isin: "US0000000001"),
                CreateTestCompany("Test Company 2", isin: "US0000000002")
            );
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new CompanyDbContext(options))
        {
            var repository = new CompanyRepository(context);
            var companies = await repository.GetAllAsync();

            // Assert
            Assert.Equal(2, companies.Count());
            Assert.Contains(companies, c => c.Name == "Test Company 1");
            Assert.Contains(companies, c => c.Name == "Test Company 2");
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCompaniesExist()
    {
        // Arrange
        var options = GetDbContextOptions();

        // Act
        using (var context = new CompanyDbContext(options))
        {
            var repository = new CompanyRepository(context);
            var companies = await repository.GetAllAsync();

            // Assert
            companies.Should().NotBeNull();
            companies.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCompany_WhenCompanyExists()
    {
        // Arrange
        var options = GetDbContextOptions();
        var testCompany = CreateTestCompany("Test Company");

        using (var context = new CompanyDbContext(options))
        {
            context.Companies.Add(testCompany);
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new CompanyDbContext(options))
        {
            var repository = new CompanyRepository(context);
            var company = await repository.GetByIdAsync(testCompany.Id);

            // Assert
            Assert.NotNull(company);
            Assert.Equal("Test Company", company.Name);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCompanyDoesNotExist()
    {
        // Arrange
        var options = GetDbContextOptions();
        var nonExistentId = Guid.NewGuid();

        // Act
        using (var context = new CompanyDbContext(options))
        {
            var repository = new CompanyRepository(context);
            var company = await repository.GetByIdAsync(nonExistentId);

            // Assert
            company.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetByIsinAsync_ShouldReturnCompany_WhenCompanyExists()
    {
        // Arrange
        var options = GetDbContextOptions();
        var isin = "US1234567890";
        var testCompany = CreateTestCompany("ISIN Test Company", isin: isin);

        using (var context = new CompanyDbContext(options))
        {
            context.Companies.Add(testCompany);
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new CompanyDbContext(options))
        {
            var repository = new CompanyRepository(context);
            var company = await repository.GetByIsinAsync(isin);

            // Assert
            company.Should().NotBeNull();
            company!.Name.Should().Be("ISIN Test Company");
            company.ISIN.Should().Be(isin);
        }
    }

    [Fact]
    public async Task GetByIsinAsync_ShouldReturnNull_WhenCompanyDoesNotExist()
    {
        // Arrange
        var options = GetDbContextOptions();
        var nonExistentIsin = "US9999999999";

        // Act
        using (var context = new CompanyDbContext(options))
        {
            var repository = new CompanyRepository(context);
            var company = await repository.GetByIsinAsync(nonExistentIsin);

            // Assert
            company.Should().BeNull();
        }
    }

    [Fact]
    public async Task AddAsync_ShouldAddCompany()
    {
        // Arrange
        var options = GetDbContextOptions();
        var newCompany = CreateTestCompany("New Company");

        // Act
        using (var context = new CompanyDbContext(options))
        {
            var repository = new CompanyRepository(context);
            await repository.AddAsync(newCompany);
            await context.SaveChangesAsync();
        }

        // Assert
        using (var context = new CompanyDbContext(options))
        {
            Assert.Equal(1, await context.Companies.CountAsync());
            var company = await context.Companies.FirstAsync();
            Assert.Equal("New Company", company.Name);
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCompany()
    {
        // Arrange
        var options = GetDbContextOptions();
        var company = CreateTestCompany("Original Company");

        using (var context = new CompanyDbContext(options))
        {
            context.Companies.Add(company);
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new CompanyDbContext(options))
        {
            var repository = new CompanyRepository(context);
            var storedCompany = await context.Companies.FirstAsync();

            // Update the company
            var updateResult = storedCompany.Update(
                "Updated Company",
                "UPD",
                "NASDAQ",
                storedCompany.ISIN,
                "https://updated-company.com"
            );

            updateResult.IsSuccess.Should().BeTrue();

            await repository.UpdateAsync(storedCompany);
            await context.SaveChangesAsync();
        }

        // Assert
        using (var context = new CompanyDbContext(options))
        {
            var updatedCompany = await context.Companies.FirstAsync();
            updatedCompany.Name.Should().Be("Updated Company");
            updatedCompany.Ticker.Should().Be("UPD");
            updatedCompany.Exchange.Should().Be("NASDAQ");
            updatedCompany.Website.Should().Be("https://updated-company.com");
        }
    }
}