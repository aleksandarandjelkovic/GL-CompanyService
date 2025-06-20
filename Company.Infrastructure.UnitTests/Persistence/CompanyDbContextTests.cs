using Company.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Company.Infrastructure.UnitTests.Persistence;

public class CompanyDbContextTests
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
    public async Task SaveChanges_ShouldPersistCompany()
    {
        // Arrange
        var options = GetDbContextOptions();
        var entity = CreateTestCompany("Test Company");

        using (var context = new CompanyDbContext(options))
        {
            // Act
            context.Companies.Add(entity);
            await context.SaveChangesAsync();
        }

        // Assert
        using (var context = new CompanyDbContext(options))
        {
            var savedEntity = await context.Companies.FirstAsync();
            Assert.NotEqual(Guid.Empty, savedEntity.Id);
            Assert.Equal("Test Company", savedEntity.Name);
            Assert.Equal("TEST", savedEntity.Ticker);
            Assert.Equal("NYSE", savedEntity.Exchange);
            Assert.Equal("US0000000001", savedEntity.ISIN);
        }
    }

    [Fact]
    public async Task SaveChanges_ShouldUpdateCompany()
    {
        // Arrange
        var options = GetDbContextOptions();
        var entity = CreateTestCompany("Test Company");

        using (var context = new CompanyDbContext(options))
        {
            context.Companies.Add(entity);
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new CompanyDbContext(options))
        {
            var savedEntity = await context.Companies.FirstAsync();
            var updateResult = savedEntity.Update("Updated Company", savedEntity.Ticker, savedEntity.Exchange, savedEntity.ISIN, "https://updated.com");
            Assert.True(updateResult.IsSuccess);
            await context.SaveChangesAsync();
        }

        // Assert
        using (var context = new CompanyDbContext(options))
        {
            var updatedEntity = await context.Companies.FirstAsync();
            Assert.Equal("Updated Company", updatedEntity.Name);
            Assert.Equal("https://updated.com", updatedEntity.Website);
        }
    }
}