using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Company.Tests.Integration.Authentication
{
    public class AuthHelper
    {
        private readonly HttpClient _httpClient;
        private readonly AuthSettings _authSettings;
        
        public AuthHelper(AuthSettings authSettings)
        {
            _authSettings = authSettings;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Gets an access token using client credentials flow
        /// </summary>
        public async Task<string> GetAccessTokenAsync()
        {
            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _authSettings.ClientId,
                ["client_secret"] = _authSettings.ClientSecret,
                ["scope"] = _authSettings.Scope
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _authSettings.TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(tokenRequest)
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Failed to get access token");
        }

        /// <summary>
        /// Adds an authorization token to the HTTP client
        /// </summary>
        public async Task AuthenticateClientAsync(HttpClient client)
        {
            var token = await GetAccessTokenAsync();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Creates a new request with authorization header
        /// </summary>
        public async Task<HttpRequestMessage> CreateAuthenticatedRequestAsync(HttpMethod method, string requestUri)
        {
            var token = await GetAccessTokenAsync();
            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }

        private class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }
            
            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
            
            [JsonPropertyName("token_type")]
            public string? TokenType { get; set; }
        }
    }

    public class AuthSettings
    {
        public string TokenEndpoint { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
    }
} 