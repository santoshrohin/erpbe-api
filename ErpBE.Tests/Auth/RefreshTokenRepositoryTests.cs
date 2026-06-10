using ErpBE.Domain.Auth;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Auth;

/// <summary>
/// Integration tests for RefreshTokenRepository.
/// Verifies token creation, rotation (validate + invalidate old), and revocation.
/// Each test starts with a clean USER_REFRESH_TOKEN table (Respawner wipes it).
/// </summary>
[Collection(nameof(IntegrationFixture))]
public class RefreshTokenRepositoryTests : IntegrationTestBase
{
    private IRefreshTokenRepository Repo => GetService<IRefreshTokenRepository>();

    // ─── Helper — get TestUser code via login ────────────────────────────────
    private async Task<int> GetTestUserCodeAsync()
    {
        var response = await Mediator.Send(new ErpBE.Application.Auth.Queries.Login.LoginRequest
        {
            Username          = "TestUser",
            Password          = "Test@123",
            CompanyId         = 1,
            FinancialYearCode = -2147483641
        });
        return response.UserCode;
    }

    // ─── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ReturnsPositiveId()
    {
        var userCode = await GetTestUserCodeAsync();
        var id = await Repo.CreateAsync(
            userCode,
            tokenHash:  "hash_aaa",
            expiresAt:  DateTime.UtcNow.AddDays(7));

        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateAsync_MultipleTokensForSameUser_AllSucceed()
    {
        var userCode = await GetTestUserCodeAsync();

        var id1 = await Repo.CreateAsync(userCode, "hash_1", DateTime.UtcNow.AddDays(7));
        var id2 = await Repo.CreateAsync(userCode, "hash_2", DateTime.UtcNow.AddDays(7));

        id1.Should().BeGreaterThan(0);
        id2.Should().BeGreaterThan(0);
        id1.Should().NotBe(id2);
    }

    // ─── ValidateAndRotateAsync ──────────────────────────────────────────────

    [Fact]
    public async Task ValidateAndRotateAsync_ValidToken_ReturnsUserCode()
    {
        var userCode = await GetTestUserCodeAsync();
        await Repo.CreateAsync(userCode, "token_rotate_valid", DateTime.UtcNow.AddDays(7));

        var result = await Repo.ValidateAndRotateAsync("token_rotate_valid", "token_rotate_new");

        result.Should().Be(userCode);
    }

    [Fact]
    public async Task ValidateAndRotateAsync_AfterRotation_OldTokenIsInvalidated()
    {
        var userCode = await GetTestUserCodeAsync();
        await Repo.CreateAsync(userCode, "token_old_for_rotate", DateTime.UtcNow.AddDays(7));

        // First rotation succeeds
        await Repo.ValidateAndRotateAsync("token_old_for_rotate", "token_new_after_rotate");

        // Second use of the old token should fail (it's revoked)
        var result = await Repo.ValidateAndRotateAsync("token_old_for_rotate", "token_attacker_new");

        result.Should().BeNull("old token must be invalid after rotation");
    }

    [Fact]
    public async Task ValidateAndRotateAsync_ExpiredToken_ReturnsNull()
    {
        var userCode = await GetTestUserCodeAsync();
        // Insert token that expired one hour ago
        await Repo.CreateAsync(userCode, "token_expired", DateTime.UtcNow.AddHours(-1));

        var result = await Repo.ValidateAndRotateAsync("token_expired", "token_new");

        result.Should().BeNull("expired token must not validate");
    }

    [Fact]
    public async Task ValidateAndRotateAsync_NonExistentToken_ReturnsNull()
    {
        var result = await Repo.ValidateAndRotateAsync("nonexistent_token_hash_xyz", "new_hash");
        result.Should().BeNull();
    }

    // ─── RevokeAllForUserAsync ───────────────────────────────────────────────

    [Fact]
    public async Task RevokeAllForUserAsync_TokensAreInvalidatedAfterRevoke()
    {
        var userCode = await GetTestUserCodeAsync();

        await Repo.CreateAsync(userCode, "token_to_revoke_1", DateTime.UtcNow.AddDays(7));
        await Repo.CreateAsync(userCode, "token_to_revoke_2", DateTime.UtcNow.AddDays(7));

        // Revoke all
        await Repo.RevokeAllForUserAsync(userCode);

        // Both tokens should now be invalid
        var r1 = await Repo.ValidateAndRotateAsync("token_to_revoke_1", "new_1");
        var r2 = await Repo.ValidateAndRotateAsync("token_to_revoke_2", "new_2");

        r1.Should().BeNull("token should be revoked");
        r2.Should().BeNull("token should be revoked");
    }

    [Fact]
    public async Task RevokeAllForUserAsync_NoTokens_DoesNotThrow()
    {
        // RevokeAll on a user with no tokens should not throw
        var act = async () => await Repo.RevokeAllForUserAsync(99999);
        await act.Should().NotThrowAsync();
    }
}
