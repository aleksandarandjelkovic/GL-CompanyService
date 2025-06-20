# Company Service

A .NET Clean Architecture project with API, Auth, and Client services.

## Prerequisites

- Docker
- Docker Compose

## Running the Application

To run the entire application (API, Auth, PostgreSQL database, client, and all tests):

```bash
docker-compose up
```

Or to run in detached mode:

```bash
docker-compose up -d
```

This will start all services:

- **Company API**: http://localhost:5000
- **Auth Service**: http://localhost:5001
- **Client**: http://localhost:4200
- **PostgreSQL**: localhost:5432
- **Unit Tests**: Runs automatically
- **Integration Tests**: Runs automatically
- **Coverage Report**: Generated automatically

The test results and coverage reports will be available in the `TestResults` directory.

## Running Specific Services

If you want to run only specific services:

```bash
# Run only the unit tests
docker-compose run --rm unit-tests

# Run only the integration tests
docker-compose run --rm integration-tests

# Generate only the coverage report
docker-compose run --rm coverage
```

## Project Structure

- **Company.Api**: REST API for the company service
- **Company.Auth**: Authentication service using Duende IdentityServer
- **Company.Domain**: Domain models and business logic
- **Company.Application**: Application services and use cases
- **Company.Infrastructure**: Data access and external services
- **Company.Api.UnitTests**: Unit tests for the API
- **Company.Infrastructure.UnitTests**: Unit tests for the infrastructure layer
- **Company.Application.UnitTests**: Unit tests for the application layer
- **Company.Api.IntegrationTests**: Integration tests for the API

## Development

### API Endpoints

- `GET /api/companies`: Get all companies
- `GET /api/companies/{id}`: Get a company by ID
- `POST /api/companies`: Create a new company
- `PUT /api/companies/{id}`: Update a company
- `DELETE /api/companies/{id}`: Delete a company

### Authentication

The API is secured with OAuth 2.0. To access protected endpoints, you need to:

1. Get a token from the Auth service
2. Include the token in the Authorization header of your requests

## Testing Strategy

This section outlines the testing approach for the Company Service application, including the structure of test projects, best practices, and guidelines for writing and running tests.

### Test Project Structure

The solution contains the following test projects:

1. **Company.Api.UnitTests**
   - Unit tests for API controllers
   - Tests controller logic in isolation with mocked dependencies

2. **Company.Application.UnitTests**
   - Unit tests for application services, validators, and DTOs
   - Tests business logic in isolation

3. **Company.Infrastructure.UnitTests**
   - Unit tests for repositories and database operations
   - Uses in-memory database for testing

4. **Company.Api.IntegrationTests**
   - Integration tests for API endpoints
   - Uses Testcontainers for PostgreSQL database
   - Tests the full request-response cycle

### Testing Technologies

- **xUnit**: Test framework
- **FluentAssertions**: Fluent assertion library
- **Moq**: Mocking framework
- **Testcontainers**: Container management for integration tests
- **Microsoft.AspNetCore.Mvc.Testing**: WebApplicationFactory for API testing

### Test Patterns and Best Practices

#### Unit Tests

- Follow the Arrange-Act-Assert pattern
- Use descriptive test names that explain the scenario and expected outcome
- Mock external dependencies
- Test one behavior per test method
- Use data-driven tests for testing multiple scenarios

#### Integration Tests

- Use Testcontainers for database integration tests
- Clean up test data after each test
- Use collection fixtures to control parallelization
- Use TestDataFactory for consistent test data creation

### Test Data Management

The solution includes a `TestDataFactory` class that provides methods for creating test data consistently across all test projects. This factory helps ensure that test data follows the same patterns and constraints.

```csharp
// Example of using TestDataFactory
var company = TestDataFactory.CreateTestCompany(
    nameSuffix: "Test",
    ticker: "TST",
    exchange: "NYSE",
    isin: "US1234567890",
    website: "https://test-company.com"
);
```

### Running Tests

#### Running Tests Locally

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test ./Company.Api.UnitTests

# Run with filter
dotnet test --filter "Category=UnitTest"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

#### Running Tests in Docker

The solution includes Docker support for running tests in containers:

```bash
# Run tests in Docker
docker-compose up test-service
```

### Test Parallelization

Tests are configured to run in parallel with the following constraints:

- Tests within the same collection do not run in parallel with each other
- Different collections can run in parallel
- Tests that use the database are grouped in collections with `DisableParallelization = true`
- In-memory database tests can run in parallel

### Test Categories

Tests are categorized using xUnit traits:

```csharp
[Trait("Category", "UnitTest")]
[Trait("Category", "IntegrationTest")]
[Trait("Category", "RepositoryTest")]
```

### Continuous Integration

Tests are automatically run in the CI pipeline on pull requests and merges to the main branch. Test results and coverage reports are published as artifacts.

## Troubleshooting

### Docker Issues

If you encounter Docker-related issues:

```bash
# Remove all containers and volumes
docker-compose down -v

# Rebuild all images
docker-compose build --no-cache

# Start services again
docker-compose up -d
```

### Database Issues

If you need to reset the database:

```bash
# Stop all services
docker-compose down

# Remove the PostgreSQL volume
docker volume rm gl-companyservice_postgres-data

# Start services again
docker-compose up -d
```

### Testing Issues

1. **Docker Container Issues**
   - Ensure Docker is running
   - Check container logs: `docker logs <container_id>`
   - Increase resource limits if needed

2. **Database Connection Issues**
   - Check connection strings
   - Verify PostgreSQL is running
   - Check network connectivity

3. **Test Failures**
   - Look for test output in the logs
   - Check test data setup and cleanup
   - Verify that tests are isolated 