using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using Company.Infrastructure.Persistence;
using Npgsql;

namespace Company.Tests.Integration
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check if this request has no auth header
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("No Authorization header"));
            }
            
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header"));
            }
            
            // Create test claims and identity
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "User"),
                new Claim("aud", "companyapi") // Add audience claim to match API expectations
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            var result = AuthenticateResult.Success(ticket);
            return Task.FromResult(result);
        }
    }
    
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private readonly string _uniqueDatabaseName = $"company_test_db_{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                // Load test-specific appsettings
                var projectDir = Directory.GetCurrentDirectory();
                var testSettingsPath = Path.Combine(projectDir, "appsettings.Test.json");
                
                configBuilder.AddJsonFile(testSettingsPath, optional: false);
            });

            builder.ConfigureServices((context, services) =>
            {
                // Configure test authentication
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
                
                // Remove the application's DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<CompanyDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add test DB context using the connection string from test settings but with a unique database name
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                var builder = new NpgsqlConnectionStringBuilder(connectionString)
                {
                    Database = _uniqueDatabaseName
                };

                // Create the database
                var masterConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
                {
                    Database = "postgres" // Connect to default postgres database to create our test database
                };

                using (var connection = new NpgsqlConnection(masterConnectionString.ConnectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"CREATE DATABASE \"{_uniqueDatabaseName}\" WITH OWNER = postgres ENCODING = 'UTF8' CONNECTION LIMIT = -1;";
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (PostgresException ex) when (ex.SqlState == "42P04") // Database already exists
                        {
                            // Ignore if database already exists
                        }
                    }
                }
                
                services.AddDbContext<CompanyDbContext>(options =>
                {
                    options.UseNpgsql(builder.ConnectionString);
                });
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clean up the test database
                try
                {
                    var configuration = new ConfigurationBuilder()
                        .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Test.json"))
                        .Build();

                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    var masterConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
                    {
                        Database = "postgres" // Connect to default postgres database to drop our test database
                    };

                    using (var connection = new NpgsqlConnection(masterConnectionString.ConnectionString))
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            // Terminate any active connections to the database
                            command.CommandText = $@"
                                SELECT pg_terminate_backend(pg_stat_activity.pid)
                                FROM pg_stat_activity
                                WHERE pg_stat_activity.datname = '{_uniqueDatabaseName}'
                                AND pid <> pg_backend_pid();";
                            command.ExecuteNonQuery();

                            // Drop the database
                            command.CommandText = $"DROP DATABASE IF EXISTS \"{_uniqueDatabaseName}\";";
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cleaning up test database: {ex.Message}");
                }
            }

            base.Dispose(disposing);
        }
    }
} 