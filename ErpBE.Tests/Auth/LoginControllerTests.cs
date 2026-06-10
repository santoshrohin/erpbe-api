using System.Linq;
using System.Threading.Tasks;
using ErpBE.Application.Auth.Queries.Login;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Auth;

/// <summary>
/// Integration tests for Login functionality
/// Tests authentication and login (matches reference implementation - tests handlers directly)
/// </summary>
public class LoginControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange - Use TestUser credentials
        var loginRequest = new LoginRequest
        {
            Username = "TestUser",
            Password = "Test@123",
            CompanyId = 1,
            FinancialYearCode = -2147483641
        };

        // Act
        var response = await Mediator.Send(loginRequest);

        // Assert
        response.Should().NotBeNull();
        response.AccessToken.Should().NotBeNullOrEmpty();
        response.Username.Should().Be("TestUser");
        response.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldThrowUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "invalid",
            Password = "invalid",
            CompanyId = 1,
            FinancialYearCode = -2147483641
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await Mediator.Send(loginRequest));
    }

    [Fact]
    public async Task Login_WithEmptyUsername_ShouldFailValidation()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "",
            Password = "Test@123",
            CompanyId = 1,
            FinancialYearCode = -2147483641
        };

        // Act & Assert - Validation should fail (handled by MediatR ValidationBehavior)
        await Assert.ThrowsAnyAsync<Exception>(async () =>
            await Mediator.Send(loginRequest));
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ShouldFailValidation()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "TestUser",
            Password = "",
            CompanyId = 1,
            FinancialYearCode = -2147483641
        };

        // Act & Assert - Validation should fail (handled by MediatR ValidationBehavior)
        await Assert.ThrowsAnyAsync<Exception>(async () =>
            await Mediator.Send(loginRequest));
    }

    [Fact]
    public async Task Login_WithValidUsername_ButInvalidCompanyId_ShouldThrowUnauthorized()
    {
        // Arrange - Test with invalid company ID
        var loginRequest = new LoginRequest
        {
            Username = "TestUser",
            Password = "Test@123",
            CompanyId = 99999, // Invalid company ID
            FinancialYearCode = -2147483641
        };

        // Act & Assert - Should return Unauthorized for invalid company
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await Mediator.Send(loginRequest));
    }

    [Fact]
    public async Task Login_WithWhitespaceUsername_ShouldFailValidation()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "   ",
            Password = "Test@123",
            CompanyId = 1,
            FinancialYearCode = -2147483641
        };

        // Act & Assert - Validation should fail (handled by MediatR ValidationBehavior)
        await Assert.ThrowsAnyAsync<Exception>(async () =>
            await Mediator.Send(loginRequest));
    }
}
