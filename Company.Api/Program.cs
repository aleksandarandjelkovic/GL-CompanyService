using System.Runtime.CompilerServices;
using Company.Api.Extensions;
using Company.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

[assembly: InternalsVisibleTo("Company.Tests")]
[assembly: InternalsVisibleTo("Company.Api.UnitTests")]
[assembly: InternalsVisibleTo("Company.Api.IntegrationTests")]

// Make Program class public for testing
public partial class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure logging
            builder.AddSerilogLogging();

            // Configure services
            builder.AddApplicationServices();
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddSwaggerServices();

            var app = builder.Build();

            // Apply migrations automatically
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CompanyDbContext>();
                try
                {
                    dbContext.Database.Migrate();
                    Log.Information("Database migrations applied successfully");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while applying migrations");
                }
            }

            // Configure the HTTP request pipeline
            app.ConfigureMiddleware();

            // Seed the database
            app.SeedDatabase();

            app.Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
