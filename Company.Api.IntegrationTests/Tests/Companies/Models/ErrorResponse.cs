using System.Text.Json.Serialization;

namespace Company.Api.IntegrationTests.Tests.Companies.Models
{
    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;
    }
}