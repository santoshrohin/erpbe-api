using ErpBE.Application.Common;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Auth;

/// <summary>
/// Unit tests for the legacy VB-era password cipher.
/// Formula per character: (Asc(char) * 20 / 2) - 100
/// Characters are joined with "-" separators.
/// </summary>
public class LegacyEncryptionTests
{
    // ─── Encrypt ────────────────────────────────────────────────────────────

    [Fact]
    public void Encrypt_SingleChar_ReturnsCorrectValue()
    {
        // 'A' = ASCII 65: (65 * 20 / 2) - 100 = 650 - 100 = 550
        LegacyEncryption.Encrypt("A").Should().Be("550");
    }

    [Fact]
    public void Encrypt_MultiChar_JoinsWithHyphens()
    {
        // '1'=49: 390, '2'=50: 400, '3'=51: 410, '4'=52: 420
        LegacyEncryption.Encrypt("1234").Should().Be("390-400-410-420");
    }

    [Fact]
    public void Encrypt_EmptyString_ReturnsEmptyString()
    {
        LegacyEncryption.Encrypt("").Should().Be("");
    }

    [Fact]
    public void Encrypt_SameInput_ProducesSameOutput()
    {
        var first  = LegacyEncryption.Encrypt("Test@123");
        var second = LegacyEncryption.Encrypt("Test@123");
        first.Should().Be(second);
    }

    [Fact]
    public void Encrypt_DifferentInputs_ProduceDifferentOutputs()
    {
        var a = LegacyEncryption.Encrypt("password1");
        var b = LegacyEncryption.Encrypt("password2");
        a.Should().NotBe(b);
    }

    // ─── Verify ─────────────────────────────────────────────────────────────

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = LegacyEncryption.Encrypt("1234");
        LegacyEncryption.Verify("1234", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = LegacyEncryption.Encrypt("1234");
        LegacyEncryption.Verify("wrongpassword", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_EmptyPassword_AgainstNonEmptyHash_ReturnsFalse()
    {
        var hash = LegacyEncryption.Encrypt("1234");
        LegacyEncryption.Verify("", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_EmptyPassword_AgainstEmptyHash_ReturnsTrue()
    {
        // Encrypt("") returns "" — so empty matches empty
        LegacyEncryption.Verify("", "").Should().BeTrue();
    }

    [Fact]
    public void Verify_CaseSensitive_ReturnsFalse()
    {
        var hash = LegacyEncryption.Encrypt("Password");
        LegacyEncryption.Verify("password", hash).Should().BeFalse();
    }

    // ─── IsBcryptHash ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("$2a$12$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy")]
    [InlineData("$2b$10$abcdefghijklmnopqrstuuABCDEFGHIJKLMNOPQRSTUVWXYZ01234")]
    public void IsBcryptHash_BcryptPrefixes_ReturnsTrue(string hash)
    {
        LegacyEncryption.IsBcryptHash(hash).Should().BeTrue();
    }

    [Theory]
    [InlineData("390-400-410-420")]
    [InlineData("550")]
    [InlineData("")]
    [InlineData("plaintext")]
    [InlineData("$1$somehash")]
    public void IsBcryptHash_NonBcryptStrings_ReturnsFalse(string hash)
    {
        LegacyEncryption.IsBcryptHash(hash).Should().BeFalse();
    }
}
