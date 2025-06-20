using Company.Api.IntegrationTests.Infrastructure;
using Company.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Company.Api.IntegrationTests.Fixtures;

/// <summary>
/// Web application factory for integration tests
/// </summary>
public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgresContainerFixture _postgresFixture;

    public ApiWebApplicationFactory()
    {
        _postgresFixture = new PostgresContainerFixture();
        _postgresFixture.InitializeAsync().GetAwaiter().GetResult();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(configBuilder =>
        {
            var config = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgresFixture.ConnectionString,
                ["Logging:LogLevel:Default"] = "Information",
                ["Logging:LogLevel:Microsoft"] = "Warning",
                ["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Information"
            };

            configBuilder.AddInMemoryCollection(config);
        });

        builder.ConfigureServices(services =>
        {
            // Remove the app's DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<CompanyDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a database context using the test PostgreSQL database
            services.AddDbContext<CompanyDbContext>(options =>
            {
                options.UseNpgsql(_postgresFixture.ConnectionString);
            });

            // Add test authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            // Add authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireCompanyApiScope", policy =>
                {
                    policy.RequireClaim("scope", "company-api");
                });

                // Apply the policy globally
                options.DefaultPolicy = options.GetPolicy("RequireCompanyApiScope")
                    ?? new AuthorizationPolicyBuilder()
                        .RequireClaim("scope", "company-api")
                        .Build();
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<CompanyDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<ApiWebApplicationFactory>>();

            try
            {
                // Ensure the database is created
                db.Database.EnsureCreated();

                // Seed the database with test data
                SeedTestData(db);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the database. Error: {Message}", ex.Message);
            }
        });
    }

    private void SeedTestData(CompanyDbContext dbContext)
    {
        // Add seed data for testing if needed
        if (!dbContext.Companies.Any())
        {
            var company1Result = Domain.Entities.Company.Create(
                "Test Company 1",
                "TEST1",
                "NYSE",
                "US0000000001",
                "https://test1.com");

            var company2Result = Domain.Entities.Company.Create(
                "Test Company 2",
                "TEST2",
                "NASDAQ",
                "US0000000002",
                "https://test2.com");

            if (company1Result.IsSuccess && company2Result.IsSuccess)
            {
                dbContext.Companies.Add(company1Result.Value!);
                dbContext.Companies.Add(company2Result.Value!);
                dbContext.SaveChanges();
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _postgresFixture.DisposeAsync().GetAwaiter().GetResult();
        }

        base.Dispose(disposing);
    }
}