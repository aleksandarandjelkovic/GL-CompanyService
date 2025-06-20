using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Company.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests that provides common functionality
/// </summary>
public abstract class IntegrationTest : IClassFixture<IntegrationTestWebApplicationFactory<Program>>
{
    protected readonly IntegrationTestWebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly ILogger Logger;

    protected IntegrationTest(IntegrationTestWebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger(GetType());
    }

    protected async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        try
        {
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException($"Failed to deserialize response to {typeof(T).Name}");
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "Error deserializing response: {Content}", content);
            throw;
        }
    }

    protected StringContent CreateJsonContent(object data)
    {
        return new StringContent(
            JsonSerializer.Serialize(data),
            Encoding.UTF8,
            "application/json");
    }

    protected T GetService<T>() where T : class
    {
        using var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }
}