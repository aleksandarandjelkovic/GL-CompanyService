using System.ComponentModel.DataAnnotations;

namespace Company.Application.DTOs;

/// <summary>
/// Represents the request data for creating a new company.
/// </summary>
public class CreateCompanyRequest
{
    /// <summary>
    /// Gets or sets the name of the company.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stock ticker symbol of the company.
    /// </summary>
    public string Ticker { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stock exchange where the company is listed.
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the International Securities Identification Number (ISIN) of the company.
    /// </summary>
    public string ISIN { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the website of the company (optional).
    /// </summary>
    [Url(ErrorMessage = "Website must be a valid URL")]
    public string? Website { get; set; }
}