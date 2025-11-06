namespace ErpBE.Tests.Integration;

/// <summary>
/// Collection definition for integration tests
/// This class has no code, and is never created. Its purpose is simply
/// to be the place to apply [CollectionDefinition] and all the
/// ICollectionFixture<> interfaces.
/// </summary>
[CollectionDefinition(nameof(IntegrationFixture))]
public class IntegrationCollectionDefinition : ICollectionFixture<IntegrationFixture> { }

