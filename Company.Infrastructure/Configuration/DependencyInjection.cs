using Company.Domain.Interfaces;
using Company.Infrastructure.Persistence;
using Company.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Infrastructure.Configuration;

/// <summary>
/// Configuration extensions for registering infrastructure layer services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure layer services to the service collection
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure DbContext with PostgreSQL
        services.AddDbContext<CompanyDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(CompanyDbContext).Assembly.FullName)));

        // Register repositories
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        return services;
    }
}