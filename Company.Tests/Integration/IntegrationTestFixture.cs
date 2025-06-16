using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Company.Tests.Integration
{
    public class IntegrationTestFixture : IAsyncLifetime, IDisposable
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        public HttpClient Client { get; }
        public HttpClient AuthenticatedClient { get; }
        
        public IntegrationTestFixture()
        {
            _factory = new CustomWebApplicationFactory<Program>();
            
            // Create unauthenticated client
            Client = _factory.CreateClient();
            
            // Create authenticated client
            AuthenticatedClient = _factory.CreateClient();
            AddAuthenticationToClient(AuthenticatedClient);
        }
        
        public HttpClient CreateClientWithoutAuthentication()
        {
            return _factory.CreateClient();
        }

        public IServiceProvider Services => _factory.Services;
        
        private void AddAuthenticationToClient(HttpClient client)
        {
            // Add a mock JWT token or other auth mechanism
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");
        }

        public async Task InitializeAsync()
        {
            // Apply migrations before running tests
            await DatabaseUtilities.InitializeDatabaseAsync(_factory.Services);
        }

        public async Task DisposeAsync()
        {
            // Cleanup after tests
            await DatabaseUtilities.ResetDatabaseAsync(_factory.Services);
        }

        public void Dispose()
        {
            Client.Dispose();
            AuthenticatedClient.Dispose();
            _factory.Dispose();
            GC.SuppressFinalize(this);
        }
    }
} 