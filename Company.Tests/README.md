# Company Tests Project

This project contains unit and integration tests for the Company API.

## Project Structure

- **/UnitTests**: Contains all unit tests that test individual components in isolation
  - **/Controllers**: Tests for API controllers
  - **/Services**: Tests for application services
  - **/Validators**: Tests for FluentValidation validators
- **/Integration**: Contains integration tests that test multiple components together
  - **/Companies**: API endpoint tests for Company resources
- **/Common**: Contains shared test utilities and helper methods

## Best Practices

1. **Naming Conventions**:
   - Test classes should be named `{ClassUnderTest}Tests`
   - Test methods should follow the pattern `{MethodName}_{Scenario}_{ExpectedResult}`

2. **Test Structure**:
   - Use the Arrange-Act-Assert pattern
   - Group related tests with #region for better readability
   - Keep tests focused on single behaviors

3. **Test Isolation**:
   - Unit tests should not depend on external resources
   - Use mocks or stubs for dependencies
   - Each test should be independent and not rely on other tests

4. **Test Coverage**:
   - Test both happy paths and error conditions
   - Ensure edge cases are covered
   - Aim for high code coverage but prioritize test quality over quantity

## Running Tests

### Unit Tests

```bash
dotnet test --filter "FullyQualifiedName~UnitTests"
```

### Integration Tests

```bash
# Start the PostgreSQL database container first
docker compose up -d postgres

# Then run the integration tests
dotnet test --filter "FullyQualifiedName~Integration"
```

### All Tests

```bash
dotnet test
```

## Authentication Testing

Authentication is tested using a custom `TestAuthHandler` which validates that:

1. Protected endpoints return 401 Unauthorized when called without authentication
2. Protected endpoints return 200 OK when called with valid authentication

## Test Data Cleanup

Integration tests automatically clean up test data to ensure test isolation. Each test runs with a clean state. 