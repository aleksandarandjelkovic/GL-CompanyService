using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Company.Api.IntegrationTests.Infrastructure;

/// <summary>
/// WebApplicationFactory for integration tests that uses Testcontainers for PostgreSQL
/// </summary>
public class IntegrationTestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
    private readonly PostgresContainerFixture _postgresFixture;
    private readonly ILogger<IntegrationTestWebApplicationFactory<TProgram>> _logger;
    private bool _initialized = false;

    public PostgresContainerFixture PostgresFixture => _postgresFixture;

    public IntegrationTestWebApplicationFactory()
    {
        _logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<IntegrationTestWebApplicationFactory<TProgram>>();

        _postgresFixture = new PostgresContainerFixture();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        try
        {
            _logger.LogInformation("Configuring web host for integration tests");

            builder.ConfigureAppConfiguration(configBuilder =>
            {
                var configDict = new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _postgresFixture.ConnectionString
                };

                configBuilder.AddInMemoryCollection(configDict);
            });

            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<Company.Infrastructure.Persistence.CompanyDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add DbContext using the test container
                services.AddDbContext<Company.Infrastructure.Persistence.CompanyDbContext>(options =>
                {
                    options.UseNpgsql(_postgresFixture.ConnectionString);
                });

                // Ensure database is created and migrated
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<Company.Infrastructure.Persistence.CompanyDbContext>();

                try
                {
                    _logger.LogInformation("Ensuring database is created and migrated");
                    db.Database.EnsureCreated();

                    // Apply migrations if available
                    try
                    {
                        db.Database.Migrate();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error applying migrations. This may be expected if using EnsureCreated.");
                    }

                    _logger.LogInformation("Database setup completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting up database for integration tests");
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring web host for integration tests");
            throw;
        }
    }

    public async Task InitializeAsync()
    {
        if (!_initialized)
        {
            _logger.LogInformation("Initializing integration test factory");
            await _postgresFixture.InitializeAsync();
            _initialized = true;
            _logger.LogInformation("Integration test factory initialized successfully");
        }
    }

    public new async Task DisposeAsync()
    {
        _logger.LogInformation("Disposing integration test factory");
        await _postgresFixture.DisposeAsync();
        base.Dispose();
        _logger.LogInformation("Integration test factory disposed successfully");
    }
}