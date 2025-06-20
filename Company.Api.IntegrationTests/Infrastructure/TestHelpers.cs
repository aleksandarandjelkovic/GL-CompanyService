using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Company.Api.IntegrationTests.Tests.Companies.Models;

namespace Company.Api.IntegrationTests.Infrastructure
{
    public static class TestHelpers
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
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

        public static CompanyRequestBuilder CreateCompanyRequestBuilder()
        {
            return new CompanyRequestBuilder();
        }
    }
}