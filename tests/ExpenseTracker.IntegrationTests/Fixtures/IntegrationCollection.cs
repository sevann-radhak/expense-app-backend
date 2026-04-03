namespace ExpenseTracker.IntegrationTests.Fixtures;

[CollectionDefinition("Integration", DisableParallelization = true)]
public sealed class IntegrationCollection : ICollectionFixture<IntegrationHostFixture>;
