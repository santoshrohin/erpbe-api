using AutoFixture;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using static ErpBE.Tests.Integration.IntegrationFixture;

namespace ErpBE.Tests.Integration;

/// <summary>
/// Base class for integration tests using ServiceProvider (matches reference implementation)
/// Tests use test database connection (separate from production) and Respawner cleans up after each test
/// SAFETY: Tests NEVER touch actual database - they use isolated Docker containers
/// </summary>
[Collection(nameof(IntegrationFixture))]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private static ServiceProvider _serviceProvider = default!;
    private readonly List<IServiceScope> _serviceScopes = [];
    protected Fixture Fixture { get; private set; } = new();
    protected ISender Mediator { get; private set; } = default!;

    /// <summary>
    /// This will be executed before each test
    /// </summary>
    public virtual Task InitializeAsync()
    {
        _serviceProvider = BuildServiceProvider();

        IServiceScope serviceScope;
        serviceScope = _serviceProvider.CreateScope();
        _serviceScopes.Add(serviceScope);

        Mediator = serviceScope.ServiceProvider.GetRequiredService<ISender>();
        Fixture = new Fixture();

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await ResetDatabaseAsync();
        _serviceScopes.ForEach(s => s.Dispose());
        await _serviceProvider.DisposeAsync();
    }

    public static TServices GetServices<TServices>() where TServices : class
    {
        return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<TServices>();
    }

    protected T GetService<T>() where T : class
    {
        return _serviceScopes.Last().ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets authentication token for testing using MediatR
    /// Uses dedicated TestUser account with Admin role
    /// Note: TestUser must be created using Setup_Test_User.sql script
    /// </summary>
    protected async Task<string> GetAuthTokenAsync()
    {
        var loginRequest = new ErpBE.Application.Auth.Queries.Login.LoginRequest
        {
            Username = "TestUser",
            Password = "Test@123",
            CompanyId = 1,
            FinancialYearCode = -2147483641
        };

        var response = await Mediator.Send(loginRequest);
        
        if (response == null || string.IsNullOrEmpty(response.Token))
        {
            throw new System.Exception($"Login failed for TestUser. " +
                "Make sure to run 'Setup_Test_User.sql' script first!");
        }

        return response.Token;
    }
}
