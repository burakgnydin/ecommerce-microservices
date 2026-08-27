namespace PaymentService.IntegrationTests.Fixtures;

/// <summary>
/// Shares a single <see cref="DatabaseFixture"/> (and its container) across all integration test
/// classes, and keeps them running sequentially against it instead of in parallel.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<DatabaseFixture>;
