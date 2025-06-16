using System;
using System.Text.Json.Serialization;

namespace Company.Tests.Integration.Companies.Models
{
    public class CompanyDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("ticker")]
        public string Ticker { get; set; } = string.Empty;
        
        [JsonPropertyName("exchange")]
        public string Exchange { get; set; } = string.Empty;
        
        [JsonPropertyName("isin")]
        public string ISIN { get; set; } = string.Empty;
        
        [JsonPropertyName("website")]
        public string? Website { get; set; }
    }
    
    public class CreateCompanyRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("ticker")]
        public string Ticker { get; set; } = string.Empty;
        
        [JsonPropertyName("exchange")]
        public string Exchange { get; set; } = string.Empty;
        
        [JsonPropertyName("isin")]
        public string ISIN { get; set; } = string.Empty;
        
        [JsonPropertyName("website")]
        public string? Website { get; set; }
    }
    
    public class UpdateCompanyRequest : CreateCompanyRequest
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
    }
} 