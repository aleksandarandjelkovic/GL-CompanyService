namespace Company.Domain.Interfaces;

/// <summary>
/// Repository interface for Company entity operations
/// </summary>
public interface ICompanyRepository
{
    /// <summary>
    /// Gets a company by its unique identifier
    /// </summary>
    /// <param name="id">The company's GUID</param>
    /// <returns>The company if found, null otherwise</returns>
    Task<Entities.Company?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a company by its ISIN
    /// </summary>
    /// <param name="isin">International Securities Identification Number</param>
    /// <returns>The company if found, null otherwise</returns>
    Task<Entities.Company?> GetByIsinAsync(string isin);

    /// <summary>
    /// Gets all companies
    /// </summary>
    /// <returns>List of all companies</returns>
    Task<List<Entities.Company>> GetAllAsync();

    /// <summary>
    /// Adds a new company to the repository
    /// </summary>
    /// <param name="company">The company to add</param>
    Task AddAsync(Entities.Company company);

    /// <summary>
    /// Updates an existing company
    /// </summary>
    /// <param name="company">The company with updated values</param>
    Task UpdateAsync(Entities.Company company);

    /// <summary>
    /// Checks if an ISIN is unique
    /// </summary>
    /// <param name="isin">International Securities Identification Number to check</param>
    /// <returns>True if the ISIN is unique, false otherwise</returns>
    Task<bool> IsIsinUniqueAsync(string isin);
}