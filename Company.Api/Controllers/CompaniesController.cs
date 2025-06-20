using Company.Application.DTOs;
using Company.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Company.Api.Controllers;

/// <summary>
/// API controller for managing companies.
/// Provides endpoints for creating, retrieving, and updating company records.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly ILogger<CompaniesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompaniesController"/> class.
    /// </summary>
    /// <param name="companyService">The company service.</param>
    /// <param name="logger">The logger instance.</param>
    public CompaniesController(
        ICompanyService companyService,
        ILogger<CompaniesController> logger
        )
    {
        _companyService = companyService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all companies.
    /// </summary>
    /// <returns>A list of all companies.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CompanyResponse>), 200)]
    public async Task<ActionResult<IEnumerable<CompanyResponse>>> GetAll()
    {
        var companies = await _companyService.GetAllCompaniesAsync();
        return Ok(companies);
    }

    /// <summary>
    /// Retrieves a company by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the company.</param>
    /// <returns>The company with the specified ID, or 404 if not found.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompanyResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CompanyResponse>> GetById(Guid id)
    {
        var company = await _companyService.GetCompanyByIdAsync(id);
        if (company == null)
        {
            return NotFound();
        }
        return Ok(company);
    }

    /// <summary>
    /// Retrieves a company by its ISIN (International Securities Identification Number).
    /// </summary>
    /// <param name="isin">The ISIN of the company.</param>
    /// <returns>The company with the specified ISIN, or 404 if not found.</returns>
    [HttpGet("isin/{isin}")]
    [ProducesResponseType(typeof(CompanyResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CompanyResponse>> GetByIsin(string isin)
    {
        if (string.IsNullOrWhiteSpace(isin))
        {
            return BadRequest("ISIN cannot be empty");
        }

        var company = await _companyService.GetCompanyByIsinAsync(isin);
        if (company == null)
        {
            return NotFound($"No company found with ISIN: {isin}");
        }

        return Ok(company);
    }

    /// <summary>
    /// Creates a new company.
    /// </summary>
    /// <param name="request">The request object containing company details.</param>
    /// <returns>The created company, or an error if creation fails.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CompanyResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<CompanyResponse>> Create([FromBody] CreateCompanyRequest request)
    {
        var result = await _companyService.CreateCompanyAsync(request);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to create company: {Error}", result.Error);
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Updates an existing company.
    /// </summary>
    /// <param name="id">The ID of the company to update.</param>
    /// <param name="request">The request object containing updated company details.</param>
    /// <returns>The updated company, or an error if update fails.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CompanyResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CompanyResponse>> Update(Guid id, [FromBody] UpdateCompanyRequest request)
    {
        // Validate ID match
        if (id != request.Id)
        {
            return BadRequest(new { error = "ID mismatch between URL and request body" });
        }

        try
        {
            var result = await _companyService.UpdateCompanyAsync(request);

            if (result.IsFailure)
            {
                _logger.LogWarning("Failed to update company: {Error}", result.Error);
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Domain.Common.Exceptions.EntityNotFoundException ex)
        {
            _logger.LogWarning("Company not found: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing company.
    /// </summary>
    /// <param name="request">The request object containing updated company details.</param>
    /// <returns>The updated company, or an error if update fails.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(CompanyResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CompanyResponse>> Update([FromBody] UpdateCompanyRequest request)
    {
        try
        {
            var result = await _companyService.UpdateCompanyAsync(request);

            if (result.IsFailure)
            {
                _logger.LogWarning("Failed to update company: {Error}", result.Error);
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Domain.Common.Exceptions.EntityNotFoundException ex)
        {
            _logger.LogWarning("Company not found: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
    }
}