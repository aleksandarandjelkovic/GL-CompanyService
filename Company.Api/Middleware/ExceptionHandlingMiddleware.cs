using System.Net;
using System.Text.Json;
using Company.Domain.Common.Exceptions;

namespace Company.Api.Middleware;

/// <summary>
/// Middleware to handle all unhandled exceptions and return standardized error responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (EntityNotFoundException enf)
        {
            _logger.LogWarning(enf, "Entity not found: {Message}", enf.Message);
            await WriteErrorResponseAsync(context, HttpStatusCode.NotFound, enf.Message, enf.ErrorType);
        }
        catch (DomainException dex)
        {
            _logger.LogWarning(dex, "Domain exception occurred: {Message}", dex.Message);
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, dex.Message, dex.ErrorType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await WriteErrorResponseAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, HttpStatusCode statusCode, string message, string? errorType = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var errorResponse = new
        {
            status = context.Response.StatusCode,
            message,
            errorType
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}