using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Company.Tests.Integration
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
        
        // Builder pattern for creating company requests
        public class CompanyRequestBuilder
        {
            private readonly Integration.Companies.Models.CreateCompanyRequest _request = new();
            
            public CompanyRequestBuilder WithName(string name)
            {
                _request.Name = name;
                return this;
            }
            
            public CompanyRequestBuilder WithTicker(string ticker)
            {
                _request.Ticker = ticker;
                return this;
            }
            
            public CompanyRequestBuilder WithExchange(string exchange)
            {
                _request.Exchange = exchange;
                return this;
            }
            
            public CompanyRequestBuilder WithISIN(string isin) 
            {
                _request.ISIN = isin;
                return this;
            }
            
            public CompanyRequestBuilder WithWebsite(string website)
            {
                _request.Website = website;
                return this;
            }
            
            public Integration.Companies.Models.CreateCompanyRequest Build()
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(_request.Name))
                    _request.Name = "Test Company";
                    
                if (string.IsNullOrWhiteSpace(_request.ISIN))
                    _request.ISIN = $"US{DateTime.Now.Ticks.ToString().Substring(0, 9)}";
                    
                if (string.IsNullOrWhiteSpace(_request.Ticker))
                    _request.Ticker = "TEST";
                    
                if (string.IsNullOrWhiteSpace(_request.Exchange))
                    _request.Exchange = "NASDAQ";
                    
                return _request;
            }
            
            public static implicit operator Integration.Companies.Models.CreateCompanyRequest(CompanyRequestBuilder builder) => builder.Build();
        }
    }
} 