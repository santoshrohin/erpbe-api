using AutoFixture;
using Xunit;

namespace ErpBE.Tests.Unit;

/// <summary>
/// Base class for unit tests
/// Provides AutoFixture for test data generation
/// </summary>
public class UnitTestBase
{
    protected readonly Fixture Fixture;

    public UnitTestBase()
    {
        Fixture = new Fixture();
    }
}

