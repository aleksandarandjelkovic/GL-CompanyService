using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Company.Api.IntegrationTests.Fixtures;

/// <summary>
/// Authentication handler for integration tests
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TimeProvider timeProvider)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if there's an Authorization header
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(AuthenticateResult.Fail("No Authorization header"));
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header"));
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        // Handle different token values for test scenarios
        if (token == "invalid-token")
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
        }

        if (token == "expired-token")
        {
            return Task.FromResult(AuthenticateResult.Fail("Token expired"));
        }

        if (token == "wrong-scope-token")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim("scope", "wrong-scope") // Wrong scope
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            // Return success but with wrong scope - API should check scope and return 403
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        // Default valid token
        var validClaims = new[]
        {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim("scope", "company-api")
        };

        var validIdentity = new ClaimsIdentity(validClaims, "Test");
        var validPrincipal = new ClaimsPrincipal(validIdentity);
        var validTicket = new AuthenticationTicket(validPrincipal, "Test");

        return Task.FromResult(AuthenticateResult.Success(validTicket));
    }
}