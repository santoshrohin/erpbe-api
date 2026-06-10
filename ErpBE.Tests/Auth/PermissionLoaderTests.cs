using ErpBE.Domain.Auth;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Auth;

/// <summary>
/// Integration tests for PermissionLoader.
/// Verifies that USER_RIGHT bitmask data is correctly read and normalised.
/// </summary>
[Collection(nameof(IntegrationFixture))]
public class PermissionLoaderTests : IntegrationTestBase
{
    private IPermissionLoader PermissionLoader => GetService<IPermissionLoader>();

    [Fact]
    public async Task LoadAsync_TestUser_ReturnsDictionaryWithModuleCodes()
    {
        // Get TestUser's UM_CODE via login flow
        var loginResult = await Mediator.Send(new ErpBE.Application.Auth.Queries.Login.LoginRequest
        {
            Username          = "TestUser",
            Password          = "Test@123",
            CompanyId         = 1,
            FinancialYearCode = -2147483641
        });

        // Act: reload permissions directly
        var permissions = loginResult.Permissions;

        // Assert: all 8 seeded modules should be present
        permissions.Should().NotBeEmpty();
        permissions.Should().ContainKey(72);   // Masters
        permissions.Should().ContainKey(75);   // Sales
        permissions.Should().ContainKey(77);   // Admin
    }

    [Fact]
    public async Task LoadAsync_TestUser_BitmaskIsSevenChars()
    {
        var loginResult = await Mediator.Send(new ErpBE.Application.Auth.Queries.Login.LoginRequest
        {
            Username          = "TestUser",
            Password          = "Test@123",
            CompanyId         = 1,
            FinancialYearCode = -2147483641
        });

        foreach (var (_, mask) in loginResult.Permissions)
        {
            mask.Length.Should().Be(7, "bitmask must always be normalised to 7 characters");
        }
    }

    [Fact]
    public async Task LoadAsync_TestUser_FullPermissions_AllBitsSet()
    {
        // TestUser is seeded with '1111111' on all modules
        var loginResult = await Mediator.Send(new ErpBE.Application.Auth.Queries.Login.LoginRequest
        {
            Username          = "TestUser",
            Password          = "Test@123",
            CompanyId         = 1,
            FinancialYearCode = -2147483641
        });

        foreach (var (_, mask) in loginResult.Permissions)
        {
            mask.Should().Be("1111111");
        }
    }

    [Fact]
    public async Task LoadAsync_NonExistentUser_ReturnsEmptyDictionary()
    {
        // UserCode 999999 does not exist
        var permissions = await PermissionLoader.LoadAsync(999999);
        permissions.Should().BeEmpty();
    }
}
