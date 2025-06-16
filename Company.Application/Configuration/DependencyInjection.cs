using Company.Application.Interfaces;
using Company.Application.Mapping;
using Company.Application.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Application.Configuration;

/// <summary>
/// Provides extension methods for registering application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application layer services to the service collection, including AutoMapper, FluentValidation, and ICompanyService.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register AutoMapper
        services.AddAutoMapper(typeof(CompanyMappingProfile).Assembly);

        // Register FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Register application services
        services.AddScoped<ICompanyService, CompanyService>();

        return services;
    }
}