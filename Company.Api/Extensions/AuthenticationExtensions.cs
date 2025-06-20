using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Company.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Clear default claim type mapping to preserve original token claims
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        var authority = Environment.GetEnvironmentVariable("AUTH_AUTHORITY") ?? "http://localhost:5001";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.RequireHttpsMetadata = false;
                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = "companyapi",
                    ValidateIssuer = true,
                    ValidIssuers = new[]
                    {
                        authority,
                        "http://company-auth:5000",
                        "http://localhost:5001"
                    },
                    ValidateLifetime = true,
                    // Allow some clock skew for Docker containers
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                // Configure to use discovery endpoint for key validation
                options.MetadataAddress = $"{authority}/.well-known/openid-configuration";
            });

        return services;
    }
}