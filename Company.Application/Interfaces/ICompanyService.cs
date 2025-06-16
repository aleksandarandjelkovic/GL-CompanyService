using Company.Application.Common;
using Company.Application.DTOs;

namespace Company.Application.Interfaces;

/// <summary>
/// Defines the contract for company-related operations.
/// </summary>
public interface ICompanyService
{
    /// <summary>
    /// Gets all companies.
    /// </summary>
    /// <returns>A collection of <see cref="CompanyResponse"/> objects.</returns>
    Task<IEnumerable<CompanyResponse>> GetAllCompaniesAsync();

    /// <summary>
    /// Gets a company by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the company.</param>
    /// <returns>The <see cref="CompanyResponse"/> if found; otherwise, <c>null</c>.</returns>
    Task<CompanyResponse?> GetCompanyByIdAsync(Guid id);

    /// <summary>
    /// Gets a company by its ISIN.
    /// </summary>
    /// <param name="isin">The ISIN of the company.</param>
    /// <returns>The <see cref="CompanyResponse"/> if found; otherwise, <c>null</c>.</returns>
    Task<CompanyResponse?> GetCompanyByIsinAsync(string isin);

    /// <summary>
    /// Creates a new company.
    /// </summary>
    /// <param name="request">The request data for creating a company.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> containing the created <see cref="CompanyResponse"/>.</returns>
    Task<ServiceResult<CompanyResponse>> CreateCompanyAsync(CreateCompanyRequest request);

    /// <summary>
    /// Updates an existing company.
    /// </summary>
    /// <param name="request">The request data for updating a company.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> containing the updated <see cref="CompanyResponse"/>.</returns>
    Task<ServiceResult<CompanyResponse>> UpdateCompanyAsync(UpdateCompanyRequest request);
}