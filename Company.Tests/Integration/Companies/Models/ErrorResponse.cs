using System.Text.Json.Serialization;

namespace Company.Tests.Integration.Companies.Models
{
    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
} 