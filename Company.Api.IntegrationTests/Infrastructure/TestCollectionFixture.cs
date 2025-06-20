using Company.Api.IntegrationTests.Fixtures;

namespace Company.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Collection definition for company API tests to control parallelism.
/// Tests within this collection will not run in parallel with each other,
/// but collections can run in parallel with other collections.
/// </summary>
[CollectionDefinition("Company API Tests", DisableParallelization = true)]
public class CompanyApiTestCollection : ICollectionFixture<ApiWebApplicationFactory>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

/// <summary>
/// Collection definition for infrastructure tests that use the database
/// </summary>
[CollectionDefinition("Infrastructure Database Tests", DisableParallelization = true)]
public class InfrastructureDatabaseTestCollection : ICollectionFixture<PostgresContainerFixture>
{
    // This class ensures that tests using the database don't run in parallel
    // to prevent database conflicts
}

/// <summary>
/// Collection definition for repository tests
/// </summary>
[CollectionDefinition("Repository Tests")]
public class RepositoryTestCollection
{
    // Repository tests use in-memory database so they can run in parallel
}