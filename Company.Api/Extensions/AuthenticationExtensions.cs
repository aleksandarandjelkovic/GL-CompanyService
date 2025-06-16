using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Company.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authority = Environment.GetEnvironmentVariable("AUTH_AUTHORITY") ?? "http://localhost:5001";
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = "companyapi",
                    ValidateIssuer = true,
                    ValidIssuer = authority
                };
                options.RequireHttpsMetadata = false;
            });
            
        return services;
    }
} 