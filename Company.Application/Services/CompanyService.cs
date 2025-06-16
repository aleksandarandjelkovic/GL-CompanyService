using AutoMapper;
using Company.Application.Common;
using Company.Application.DTOs;
using Company.Application.Interfaces;
using Company.Domain.Common.Exceptions;
using Company.Domain.Interfaces;

namespace Company.Application.Services;

/// <summary>
/// Provides company-related operations and business logic.
/// </summary>
public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyService"/> class.
    /// </summary>
    /// <param name="companyRepository">The company repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public CompanyService(ICompanyRepository companyRepository, IMapper mapper)
    {
        _companyRepository = companyRepository;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CompanyResponse>> GetAllCompaniesAsync()
    {
        var companies = await _companyRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CompanyResponse>>(companies);
    }

    /// <inheritdoc/>
    public async Task<CompanyResponse?> GetCompanyByIdAsync(Guid id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        return company != null ? _mapper.Map<CompanyResponse>(company) : null;
    }

    /// <inheritdoc/>
    public async Task<CompanyResponse?> GetCompanyByIsinAsync(string isin)
    {
        // Normalize the ISIN (trim and convert to uppercase)
        isin = isin.Trim().ToUpperInvariant();

        var company = await _companyRepository.GetByIsinAsync(isin);
        return company != null ? _mapper.Map<CompanyResponse>(company) : null;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<CompanyResponse>> CreateCompanyAsync(CreateCompanyRequest request)
    {
        // Use the factory method to create a new Company entity
        var companyResult = Domain.Entities.Company.Create(
            request.Name,
            request.Ticker,
            request.Exchange,
            request.ISIN,
            request.Website);

        if (companyResult.IsFailure)
        {
            return ServiceResult<CompanyResponse>.Failure(companyResult.Error);
        }

        var company = companyResult.Value!;

        // Check if ISIN is unique
        if (!await _companyRepository.IsIsinUniqueAsync(company.ISIN))
        {
            throw BusinessRuleException.UniqueConstraintViolation("ISIN", company.ISIN);
        }

        await _companyRepository.AddAsync(company);

        return ServiceResult<CompanyResponse>.Success(_mapper.Map<CompanyResponse>(company));
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<CompanyResponse>> UpdateCompanyAsync(UpdateCompanyRequest request)
    {
        var company = await _companyRepository.GetByIdAsync(request.Id);
        if (company == null)
        {
            throw new EntityNotFoundException("Company", request.Id);
        }

        // Check if ISIN is being changed and already exists
        if (company.ISIN != request.ISIN &&
            !await _companyRepository.IsIsinUniqueAsync(request.ISIN))
        {
            throw BusinessRuleException.UniqueConstraintViolation("ISIN", request.ISIN);
        }

        // Use the domain entity's update method
        var updateResult = company.Update(
            request.Name,
            request.Ticker,
            request.Exchange,
            request.ISIN,
            request.Website);

        if (updateResult.IsFailure)
        {
            return ServiceResult<CompanyResponse>.Failure(updateResult.Error);
        }

        await _companyRepository.UpdateAsync(company);

        return ServiceResult<CompanyResponse>.Success(_mapper.Map<CompanyResponse>(company));
    }
}