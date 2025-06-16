# Integration Tests for Company API

This directory contains integration tests for the Company API. The tests use xUnit, WebApplicationFactory, and the real PostgreSQL database.

## Prerequisites

- .NET 9 SDK
- Docker and docker-compose for running PostgreSQL
- The API codebase follows Clean Architecture

## Setup

1. Start the PostgreSQL container:

```bash
docker-compose up -d
```

2. The tests automatically:
   - Connect to the PostgreSQL database
   - Apply EF Core migrations
   - Reset data after tests

## Configuration

### Database

The tests use a dedicated test database defined in `appsettings.Test.json`. By default, it connects to:

```
Host=localhost;Port=5432;Database=company_test_db;Username=postgres;Password=postgres;
```

If you're running the tests inside a Docker container, you may need to change `localhost` to `host.docker.internal`.

### Authentication

The tests support authentication using a real auth server. Configure your auth server details in `Integration/Authentication/appsettings.Auth.json`:

```json
{
  "Authentication": {
    "TokenEndpoint": "https://your-auth-server.com/connect/token",
    "ClientId": "company-api-tests",
    "ClientSecret": "test-secret",
    "Scope": "company-api"
  }
}
```

## Test Categories

This project contains several categories of tests:

1. **Standard API Tests** - Tests for API functionality (CRUD operations)
2. **Authentication Tests** - Tests for authentication scenarios
3. **Negative Case Tests** - Tests that verify proper error handling

## Running Tests

### From Command Line

```bash
# Run all integration tests
dotnet test Company.Tests --filter "FullyQualifiedName~Integration"

# Run specific test class
dotnet test Company.Tests --filter "FullyQualifiedName~CompanyApiIntegrationTests"

# Run authentication tests only
dotnet test Company.Tests --filter "FullyQualifiedName~AuthenticationIntegrationTests"
```

### From Docker

If you want to run the tests inside a Docker container, you can add a test stage to your Dockerfile:

```dockerfile
# Test stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS test
WORKDIR /src
COPY ["Company.Api/Company.Api.csproj", "Company.Api/"]
COPY ["Company.Application/Company.Application.csproj", "Company.Application/"]
COPY ["Company.Infrastructure/Company.Infrastructure.csproj", "Company.Infrastructure/"]
COPY ["Company.Tests/Company.Tests.csproj", "Company.Tests/"]
RUN dotnet restore "Company.Tests/Company.Tests.csproj"
COPY . .
RUN sed -i 's/Host=localhost/Host=host.docker.internal/g' Company.Tests/Integration/appsettings.Test.json
RUN dotnet build "Company.Tests/Company.Tests.csproj" --no-restore
ENTRYPOINT ["dotnet", "test", "Company.Tests/Company.Tests.csproj", "--filter", "FullyQualifiedName~Integration"]
```

Then run it with:

```bash
# Make sure your PostgreSQL container is running
docker build --target test -t company-api-tests .
docker run --network=host company-api-tests
```

## Test Structure

- `CustomWebApplicationFactory.cs`: Configures the WebApplicationFactory with test settings
- `DatabaseUtilities.cs`: Handles database migrations and cleanup
- `IntegrationTestFixture.cs`: Provides shared test context with IClassFixture
- `TestHelpers.cs`: Contains utility methods for HTTP requests and responses
- `Authentication/`: Contains authentication-related tests and utilities
- `Companies/CompanyApiIntegrationTests.cs`: Tests for company API endpoints

The tests follow Arrange-Act-Assert pattern and use FluentAssertions for readable assertions. 