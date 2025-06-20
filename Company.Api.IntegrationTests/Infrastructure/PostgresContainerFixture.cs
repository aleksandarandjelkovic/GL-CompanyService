using System.Diagnostics;
using DotNet.Testcontainers.Builders;
using Microsoft.Extensions.Logging;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Company.Api.IntegrationTests.Infrastructure;

/// <summary>
/// A test fixture that manages the lifecycle of a PostgreSQL container for integration tests
/// </summary>
public class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer? _postgresContainer;
    private readonly ILogger<PostgresContainerFixture> _logger;
    private readonly bool _useExistingDb;
    private readonly int _startupTimeoutSeconds = 60; // Configurable timeout

    public string ConnectionString { get; private set; } = string.Empty;

    public PostgresContainerFixture()
    {
        _logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<PostgresContainerFixture>();

        try
        {
            // Check if we're running in Docker environment
            _useExistingDb = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Docker";

            if (_useExistingDb)
            {
                // Use the existing PostgreSQL container from docker-compose
                _logger.LogInformation("Running in Docker environment, using existing PostgreSQL container");
                var host = "company-db"; // This matches the service name in docker-compose.yml
                var database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "CompanyServiceDb";
                var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
                var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "YourStrongPassword123!";

                ConnectionString = $"Host={host};Database={database};Username={username};Password={password}";
                _postgresContainer = null;

                _logger.LogInformation("Using connection string: {ConnectionString}", ConnectionString);
            }
            else
            {
                // Configure environment variables for Testcontainers
                Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");

                if (Environment.GetEnvironmentVariable("TESTCONTAINERS_HOST_OVERRIDE") != null)
                {
                    _logger.LogInformation("Using Docker host override: {HostOverride}",
                        Environment.GetEnvironmentVariable("TESTCONTAINERS_HOST_OVERRIDE"));
                }

                // Create PostgreSQL container with resource limits
                var builder = new PostgreSqlBuilder()
                    .WithImage("postgres:15") // Use version 15
                    .WithPortBinding(5432, true)
                    .WithDatabase("integration_test_db")
                    .WithUsername("postgres")
                    .WithPassword("postgres")
                    .WithCleanUp(true);

                // Add health check wait strategy
                builder = builder.WithWaitStrategy(
                    Wait.ForUnixContainer()
                        .UntilPortIsAvailable(5432)
                        .UntilCommandIsCompleted("pg_isready -U postgres")
                );

                _postgresContainer = builder.Build();
                _logger.LogInformation("PostgreSQL container created for local testing");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PostgreSQL container");
            throw;
        }
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_useExistingDb)
            {
                _logger.LogInformation("Using existing PostgreSQL container with connection string: {ConnectionString}", ConnectionString);

                // Verify connection to existing database
                await VerifyDatabaseConnectionAsync();
                return;
            }

            _logger.LogInformation("Starting PostgreSQL container...");

            // Use CancellationToken with timeout to avoid hanging indefinitely
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_startupTimeoutSeconds));

            try
            {
                await _postgresContainer!.StartAsync(cts.Token);
                ConnectionString = _postgresContainer.GetConnectionString();
                _logger.LogInformation("PostgreSQL container started successfully");
                _logger.LogInformation("Connection string: {ConnectionString}", ConnectionString);

                // Verify connection to container database
                await VerifyDatabaseConnectionAsync();
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("PostgreSQL container startup timed out after {Timeout} seconds", _startupTimeoutSeconds);
                throw new TimeoutException($"PostgreSQL container failed to start within {_startupTimeoutSeconds} seconds");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting PostgreSQL container");

            // Additional diagnostic information
            try
            {
                var dockerInfo = RunDockerCommand("info");
                _logger.LogInformation("Docker info: {DockerInfo}", dockerInfo);

                var dockerPs = RunDockerCommand("ps -a");
                _logger.LogInformation("Docker containers: {DockerPs}", dockerPs);

                // Check Docker disk space
                var dockerDf = RunDockerCommand("system df");
                _logger.LogInformation("Docker disk usage: {DockerDf}", dockerDf);
            }
            catch (Exception diagEx)
            {
                _logger.LogError(diagEx, "Error getting Docker diagnostic information");
            }

            throw;
        }
    }

    private async Task VerifyDatabaseConnectionAsync()
    {
        _logger.LogInformation("Verifying database connection...");

        try
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT 1";
            await cmd.ExecuteScalarAsync();
            _logger.LogInformation("Database connection verified successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to database");
            throw new InvalidOperationException("Could not connect to PostgreSQL database", ex);
        }
    }

    public async Task DisposeAsync()
    {
        if (_useExistingDb)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Stopping PostgreSQL container...");
            if (_postgresContainer != null)
            {
                // Use CancellationToken with timeout to avoid hanging indefinitely
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await _postgresContainer.DisposeAsync().AsTask().WaitAsync(cts.Token);
                _logger.LogInformation("PostgreSQL container stopped successfully");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("PostgreSQL container disposal timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping PostgreSQL container");
        }
    }

    private string RunDockerCommand(string arguments)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Docker command error: {Error}", error);
            }

            return output;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running Docker command: {Arguments}", arguments);
            return $"Error: {ex.Message}";
        }
    }
}

/// <summary>
/// Enum for resource limits in Testcontainers
/// </summary>
public static class TestcontainersResourceLimit
{
    public const string Cpu = "cpus";
    public const string Memory = "memory";
}