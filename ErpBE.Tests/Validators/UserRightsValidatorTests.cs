using ErpBE.Application.DTOs;
using ErpBE.Application.UserRights.Commands;
using ErpBE.Application.UserRights.Validators;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Validators;

public class UserRightsValidatorTests
{
    // ─── SaveUserRights ──────────────────────────────────────────────────────

    [Fact]
    public void Save_ValidCommand_PassesValidation()
    {
        var validator = new SaveUserRightsCommandValidator();
        var cmd = new SaveUserRightsCommand
        {
            UserCode = 5,
            Rights   = new() { new() { ScreenCode = 75, Bitmask = "1111000" } }
        };

        validator.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Save_EmptyBitmask_FailsValidation()
    {
        var validator = new SaveUserRightsCommandValidator();
        var cmd = new SaveUserRightsCommand
        {
            UserCode = 5,
            Rights   = new() { new() { ScreenCode = 75, Bitmask = "" } }
        };

        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Save_WrongLengthBitmask_FailsValidation()
    {
        var validator = new SaveUserRightsCommandValidator();
        var cmd = new SaveUserRightsCommand
        {
            UserCode = 5,
            Rights   = new() { new() { ScreenCode = 75, Bitmask = "111" } }
        };

        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Save_InvalidBitmaskCharacters_FailsValidation()
    {
        var validator = new SaveUserRightsCommandValidator();
        var cmd = new SaveUserRightsCommand
        {
            UserCode = 5,
            Rights   = new() { new() { ScreenCode = 75, Bitmask = "111X000" } }
        };

        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Save_ZeroUserCode_FailsValidation()
    {
        var validator = new SaveUserRightsCommandValidator();
        var cmd = new SaveUserRightsCommand { UserCode = 0, Rights = new() };

        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserCode");
    }

    // ─── CopyUserRights ──────────────────────────────────────────────────────

    [Fact]
    public void Copy_ValidCommand_PassesValidation()
    {
        var validator = new CopyUserRightsCommandValidator();
        var cmd = new CopyUserRightsCommand { FromUserCode = 3, ToUserCode = 5 };

        validator.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Copy_SameUserCode_FailsValidation()
    {
        var validator = new CopyUserRightsCommandValidator();
        var cmd = new CopyUserRightsCommand { FromUserCode = 5, ToUserCode = 5 };

        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Copy_ZeroFromUser_FailsValidation()
    {
        var validator = new CopyUserRightsCommandValidator();
        var cmd = new CopyUserRightsCommand { FromUserCode = 0, ToUserCode = 5 };

        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FromUserCode");
    }
}
