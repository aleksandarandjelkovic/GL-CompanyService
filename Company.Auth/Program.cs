using Duende.IdentityServer.Models;

var builder = WebApplication.CreateBuilder(args);

// Add CORS for Swagger UI
builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure IdentityServer
builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
    
    // Allow lowercase scopes to make it easier
    options.EmitStaticAudienceClaim = true;
    
    // Use environment-aware issuer URI
    var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    options.IssuerUri = isDocker ? "http://company-auth:5000" : "http://localhost:5001";
})
.AddInMemoryApiScopes(new[]
{
    new ApiScope("companyapi", "Company API")
})
.AddInMemoryApiResources(new[]
{
    new ApiResource("companyapi", "Company API")
    {
        Scopes = { "companyapi" }
    }
})
.AddInMemoryClients(new[]
{
    // Client for Swagger UI
    new Client
    {
        ClientId = "swagger",
        ClientName = "Swagger UI",
        AllowedGrantTypes = GrantTypes.ClientCredentials,
        ClientSecrets = { new Secret("secret".Sha256()) },
        AllowedScopes = { "companyapi" },
        AllowedCorsOrigins = { "http://localhost:5000" },
        AllowOfflineAccess = true,
        AccessTokenLifetime = 3600 * 24 // 24 hours for testing
    },
    // Original SPA client (keep for future use)
    new Client
    {
        ClientId = "angular_spa",
        ClientName = "Angular SPA",
        AllowedGrantTypes = GrantTypes.Implicit,
        AllowAccessTokensViaBrowser = true,
        RedirectUris = { 
            "http://localhost:4200/auth-callback",
            "http://localhost:5000/swagger/oauth2-redirect.html" 
        },
        AllowedScopes = { "companyapi" },
        AllowOfflineAccess = true,
        RequireClientSecret = false,
        RequireConsent = false
    }
})
.AddDeveloperSigningCredential();

var app = builder.Build();

// Use CORS before IdentityServer
app.UseCors("default");
app.UseIdentityServer();

app.MapGet("/", () => "Duende IdentityServer is running.");

app.Run();
