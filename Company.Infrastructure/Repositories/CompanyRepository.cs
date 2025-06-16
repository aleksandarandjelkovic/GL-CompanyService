using Company.Domain.Interfaces;
using Company.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Company.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of <see cref="ICompanyRepository"/>.
/// </summary>
public class CompanyRepository : ICompanyRepository
{
    private readonly CompanyDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public CompanyRepository(CompanyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<Domain.Entities.Company?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Companies.FindAsync(id);
    }

    /// <inheritdoc/>
    // to retrieve an entity by primary key
    public async Task<Domain.Entities.Company?> GetByIsinAsync(string isin)
    {
        return await _dbContext.Companies
            .AsNoTracking() // Added for read-only operations
            .FirstOrDefaultAsync(c => c.ISIN == isin.Trim().ToUpperInvariant());
    }

    /// <inheritdoc/>
    public async Task<List<Domain.Entities.Company>> GetAllAsync()
    {
        return await _dbContext.Companies
            .AsNoTracking() // Added for read-only list operations
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task AddAsync(Domain.Entities.Company company)
    {
        await _dbContext.Companies.AddAsync(company);
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Domain.Entities.Company company)
    {
        // EntityFramework will track the changes automatically
        // because the entity is already being tracked
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> IsIsinUniqueAsync(string isin)
    {
        // Normalize the ISIN for comparison
        var normalizedIsin = isin.Trim().ToUpperInvariant();

        return !await _dbContext.Companies
            .AsNoTracking()
            .AnyAsync(c => c.ISIN == normalizedIsin);
    }
}