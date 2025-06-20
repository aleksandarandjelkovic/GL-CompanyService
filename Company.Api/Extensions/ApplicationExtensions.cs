using Company.Api.Middleware;
using Company.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Company.Api.Extensions;

public static class ApplicationExtensions
{
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        // Register custom exception handling middleware early in the pipeline
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Apply CORS policy globally
        app.UseCors("AllowClientApp");

        // Enable authentication and authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Configure Swagger in development
        app.UseSwaggerServices(app.Environment);

        app.UseHttpsRedirection();
        app.MapControllers();

        // Add health check endpoint
        app.MapHealthChecks("/health");

        return app;
    }

    public static void SeedDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CompanyDbContext>();
        db.Database.Migrate();
        DbSeeder.Seed(db);
    }
}