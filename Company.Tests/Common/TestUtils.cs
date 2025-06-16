using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Company.Tests.Common
{
    public static class TestUtils
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        public static StringContent CreateJsonContent<T>(T content)
        {
            var json = JsonSerializer.Serialize(content, JsonOptions);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
        
        public static async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
        {
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        }
        
        public static async Task<string> GetResponseStringAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Creates a domain entity for testing purposes
        /// </summary>
        public static Domain.Entities.Company CreateTestCompany(string name, string ticker, string exchange, string isin, string? website = null)
        {
            var result = Domain.Entities.Company.Create(name, ticker, exchange, isin, website);
            if (result.IsSuccess)
                return result.Value!;
            
            throw new InvalidOperationException($"Failed to create test company: {result.Error}");
        }
    }
} 