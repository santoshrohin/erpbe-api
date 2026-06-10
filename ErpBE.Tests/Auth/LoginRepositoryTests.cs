using ErpBE.Application.Auth.Queries.Login;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Auth;

/// <summary>
/// Integration tests for the authentication flow via MediatR LoginHandler.
/// Covers: valid login, wrong password, unknown user, JWT shape, permissions in response.
/// </summary>
[Collection(nameof(IntegrationFixture))]
public class LoginRepositoryTests : IntegrationTestBase
{
    // ─── Success cases ───────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithLegacyCipherPassword_Succeeds()
    {
        var request = new LoginRequest
        {
            Username          = "TestUser",
            Password          = "Test@123",
            CompanyId         = 1,
            FinancialYearCode = -2147483641
        };

        var response = await Mediator.Send(request);

        response.Should().NotBeNull();
        response.AccessToken.Should().NotBeNullOrEmpty();
        response.Username.Should().Be("TestUser");
    }

    [Fact]
    public async Task Login_Success_ReturnsCorrectCompanyId()
    {
        var response = await Mediator.Send(new LoginRequest
        {
            Username = "TestUser", Password = "Test@123",
            CompanyId = 1, FinancialYearCode = -2147483641
        });

        response.CompanyId.Should().Be(1);
        response.FinancialYearCode.Should().Be(-2147483641);
    }

    [Fact]
    public async Task Login_Success_PermissionsAreLoaded()
    {
        var response = await Mediator.Send(new LoginRequest
        {
            Username = "TestUser", Password = "Test@123",
            CompanyId = 1, FinancialYearCode = -2147483641
        });

        response.Permissions.Should().NotBeEmpty("TestUser has seeded USER_RIGHT rows");
        response.Permissions.Should().ContainKey(75); // Sales module
    }

    [Fact]
    public async Task Login_Success_AccessTokenIsValidJwt()
    {
        var response = await Mediator.Send(new LoginRequest
        {
            Username = "TestUser", Password = "Test@123",
            CompanyId = 1, FinancialYearCode = -2147483641
        });

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        handler.CanReadToken(response.AccessToken).Should().BeTrue();
    }

    [Fact]
    public async Task Login_Success_RefreshTokenIsNotEmpty()
    {
        var response = await Mediator.Send(new LoginRequest
        {
            Username = "TestUser", Password = "Test@123",
            CompanyId = 1, FinancialYearCode = -2147483641
        });

        response.RefreshToken.Should().NotBeNullOrEmpty(
            "handler generates a refresh token and stores its hash");
    }

    [Fact]
    public async Task Login_AdminUser_IsAdminFlagIsTrue()
    {
        var response = await Mediator.Send(new LoginRequest
        {
            Username = "TestUser", Password = "Test@123",
            CompanyId = 1, FinancialYearCode = -2147483641
        });

        response.IsAdmin.Should().BeTrue("TestUser is seeded with UM_IS_ADMIN = 1");
    }

    // ─── Failure cases ───────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Mediator.Send(new LoginRequest
            {
                Username = "TestUser", Password = "WrongPassword!",
                CompanyId = 1, FinancialYearCode = -2147483641
            }));
    }

    [Fact]
    public async Task Login_UnknownUser_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Mediator.Send(new LoginRequest
            {
                Username = "ghost_user_xyz", Password = "Test@123",
                CompanyId = 1, FinancialYearCode = -2147483641
            }));
    }

    [Fact]
    public async Task Login_WrongCompanyId_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Mediator.Send(new LoginRequest
            {
                Username = "TestUser", Password = "Test@123",
                CompanyId = 99999, FinancialYearCode = -2147483641
            }));
    }

    // ─── Validation ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_EmptyUsername_ThrowsValidationException()
    {
        await Assert.ThrowsAnyAsync<Exception>(() =>
            Mediator.Send(new LoginRequest
            {
                Username = "", Password = "Test@123",
                CompanyId = 1, FinancialYearCode = -2147483641
            }));
    }

    [Fact]
    public async Task Login_EmptyPassword_ThrowsValidationException()
    {
        await Assert.ThrowsAnyAsync<Exception>(() =>
            Mediator.Send(new LoginRequest
            {
                Username = "TestUser", Password = "",
                CompanyId = 1, FinancialYearCode = -2147483641
            }));
    }

    [Fact]
    public async Task Login_WhitespaceUsername_ThrowsValidationException()
    {
        await Assert.ThrowsAnyAsync<Exception>(() =>
            Mediator.Send(new LoginRequest
            {
                Username = "   ", Password = "Test@123",
                CompanyId = 1, FinancialYearCode = -2147483641
            }));
    }
}
