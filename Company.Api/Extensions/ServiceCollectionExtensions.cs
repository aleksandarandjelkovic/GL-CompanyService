using Company.Application.Configuration;
using Company.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Company.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        // Configure environment-specific settings
        ConfigureEnvironmentSpecificSettings(builder);
        
        // Add MVC and API explorer
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        // Add application services
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        
        // Add CORS with more permissive configuration
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowClientApp", policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });
        
        return builder;
    }
    
    private static void ConfigureEnvironmentSpecificSettings(WebApplicationBuilder builder)
    {
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
        {
            builder.Configuration.AddJsonFile("appsettings.Docker.json", optional: false);
            
            // Replace environment variables in connection string
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            
            if (!string.IsNullOrEmpty(connectionString))
            {
                connectionString = connectionString
                    .Replace("${POSTGRES_DB}", Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "CompanyServiceDb")
                    .Replace("${POSTGRES_USER}", Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres")
                    .Replace("${POSTGRES_PASSWORD}", Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres");
                
                builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
            }
            else
            {
                Log.Warning("No DefaultConnection string found in configuration");
            }
        }
    }
} 