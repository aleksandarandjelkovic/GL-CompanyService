using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Company.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        var publicAuthUrl = "http://localhost:5001";

        services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            // Add security definition for OAuth2 client credentials flow
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    ClientCredentials = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri($"{publicAuthUrl}/connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "companyapi", "Company API" }
                        }
                    }
                },
                Description = "Client ID: swagger<br/>Client Secret: secret"
            });

            // Apply the security requirement to all operations
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    new[] { "companyapi" }
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerServices(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Company API v1");
                c.OAuthClientId("swagger");
                c.OAuthClientSecret("secret");
                c.OAuthScopes("companyapi");
                c.OAuthUsePkce();
                c.OAuthScopeSeparator(" ");

                // Enable client credentials flow
                c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
                c.EnableDeepLinking();
                c.DisplayRequestDuration();

                c.RoutePrefix = "swagger";
            });
        }

        return app;
    }
}