using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Company.Infrastructure.Persistence;

/// <summary>
/// Provides a design-time factory for creating <see cref="CompanyDbContext"/> instances.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CompanyDbContext>
{
    /// <summary>
    /// Creates a new instance of <see cref="CompanyDbContext"/> for design-time operations.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>A new <see cref="CompanyDbContext"/> instance.</returns>
    public CompanyDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .Build();

        var builder = new DbContextOptionsBuilder<CompanyDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        builder.UseNpgsql(connectionString);

        return new CompanyDbContext(builder.Options);
    }
}