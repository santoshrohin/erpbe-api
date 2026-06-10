using ErpBE.Application.DeliveryChallan.Commands;
using ErpBE.Application.DeliveryChallan.Validators;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Validators;

public class DeliveryChallanValidatorTests
{
    // ─── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidCommand_PassesValidation()
    {
        var validator = new CreateDeliveryChallanCommandValidator();
        var cmd = new CreateDeliveryChallanCommand
        {
            CompanyCode  = 1,
            CustomerCode = 1,
            ChallanDate  = DateTime.Today,
            Details      = new List<CreateDeliveryChallanDetailCommand>
            {
                new() { ItemCode = 1, OrderedQuantity = 10 }
            }
        };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Create_MissingCompanyCode_FailsValidation()
    {
        var validator = new CreateDeliveryChallanCommandValidator();
        var cmd = new CreateDeliveryChallanCommand
        {
            CompanyCode  = 0, // Invalid
            CustomerCode = 1,
            ChallanDate  = DateTime.Today,
            Details      = new List<CreateDeliveryChallanDetailCommand> { new() { ItemCode = 1, OrderedQuantity = 5 } }
        };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CompanyCode");
    }

    [Fact]
    public void Create_MissingChallanDate_FailsValidation()
    {
        var validator = new CreateDeliveryChallanCommandValidator();
        var cmd = new CreateDeliveryChallanCommand
        {
            CompanyCode  = 1,
            CustomerCode = 1,
            ChallanDate  = null,
            Details      = new List<CreateDeliveryChallanDetailCommand> { new() { ItemCode = 1, OrderedQuantity = 5 } }
        };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ChallanDate");
    }

    [Fact]
    public void Create_EmptyDetails_FailsValidation()
    {
        var validator = new CreateDeliveryChallanCommandValidator();
        var cmd = new CreateDeliveryChallanCommand
        {
            CompanyCode  = 1,
            CustomerCode = 1,
            ChallanDate  = DateTime.Today,
            Details      = new List<CreateDeliveryChallanDetailCommand>()
        };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Details");
    }

    [Fact]
    public void Create_DetailWithZeroQuantity_FailsValidation()
    {
        var validator = new CreateDeliveryChallanCommandValidator();
        var cmd = new CreateDeliveryChallanCommand
        {
            CompanyCode  = 1,
            CustomerCode = 1,
            ChallanDate  = DateTime.Today,
            Details      = new List<CreateDeliveryChallanDetailCommand>
            {
                new() { ItemCode = 1, OrderedQuantity = 0 } // Invalid: must be > 0
            }
        };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
    }

    // ─── Update ──────────────────────────────────────────────────────────────

    [Fact]
    public void Update_ValidCommand_PassesValidation()
    {
        var validator = new UpdateDeliveryChallanCommandValidator();
        var cmd = new UpdateDeliveryChallanCommand
        {
            ChallanCode  = 1,
            CompanyCode  = 1,
            CustomerCode = 1,
            ChallanDate  = DateTime.Today,
            Details      = new List<UpdateDeliveryChallanDetailCommand> { new() { ItemCode = 1, OrderedQuantity = 5 } }
        };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Update_MissingChallanCode_FailsValidation()
    {
        var validator = new UpdateDeliveryChallanCommandValidator();
        var cmd = new UpdateDeliveryChallanCommand
        {
            ChallanCode  = 0, // Invalid
            CompanyCode  = 1,
            CustomerCode = 1,
            ChallanDate  = DateTime.Today,
            Details      = new List<UpdateDeliveryChallanDetailCommand> { new() { ItemCode = 1, OrderedQuantity = 5 } }
        };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ChallanCode");
    }

    [Fact]
    public void Update_NegativeChallanCode_PassesValidation()
    {
        var validator = new UpdateDeliveryChallanCommandValidator();
        var cmd = new UpdateDeliveryChallanCommand
        {
            ChallanCode  = int.MinValue, // Negative codes are valid (IDENTITY starts at INT_MIN)
            CompanyCode  = 1,
            CustomerCode = 1,
            ChallanDate  = DateTime.Today,
            Details      = new List<UpdateDeliveryChallanDetailCommand> { new() { ItemCode = 1, OrderedQuantity = 5 } }
        };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeTrue("negative challan codes are valid as IDENTITY starts at INT_MIN");
    }

    [Fact]
    public void Delete_NegativeChallanCode_PassesValidation()
    {
        var validator = new DeleteDeliveryChallanCommandValidator();
        var cmd = new DeleteDeliveryChallanCommand { ChallanCode = int.MinValue, CompanyCode = 1 };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeTrue("negative challan codes are valid as IDENTITY starts at INT_MIN");
    }

    [Fact]
    public void Create_MissingCustomerCode_FailsValidation()
    {
        var validator = new CreateDeliveryChallanCommandValidator();
        var cmd = new CreateDeliveryChallanCommand
        {
            CompanyCode  = 1,
            CustomerCode = null,
            ChallanDate  = DateTime.Today,
            Details      = new List<CreateDeliveryChallanDetailCommand> { new() { ItemCode = 1, OrderedQuantity = 5 } }
        };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerCode");
    }

    [Fact]
    public void Create_CustomerCodeZero_FailsValidation()
    {
        var validator = new CreateDeliveryChallanCommandValidator();
        var cmd = new CreateDeliveryChallanCommand
        {
            CompanyCode  = 1,
            CustomerCode = 0,
            ChallanDate  = DateTime.Today,
            Details      = new List<CreateDeliveryChallanDetailCommand> { new() { ItemCode = 1, OrderedQuantity = 5 } }
        };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerCode");
    }

    [Fact]
    public void Create_DuplicateItemCode_FailsValidation()
    {
        var validator = new CreateDeliveryChallanCommandValidator();
        var cmd = new CreateDeliveryChallanCommand
        {
            CompanyCode  = 1,
            CustomerCode = 1,
            ChallanDate  = DateTime.Today,
            Details = new List<CreateDeliveryChallanDetailCommand>
            {
                new() { ItemCode = 1, OrderedQuantity = 5 },
                new() { ItemCode = 1, OrderedQuantity = 3 },  // duplicate
            }
        };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Duplicate item"));
    }

    [Fact]
    public void Create_TwoDistinctItems_PassesValidation()
    {
        var validator = new CreateDeliveryChallanCommandValidator();
        var cmd = new CreateDeliveryChallanCommand
        {
            CompanyCode  = 1,
            CustomerCode = 1,
            ChallanDate  = DateTime.Today,
            Details = new List<CreateDeliveryChallanDetailCommand>
            {
                new() { ItemCode = 1, OrderedQuantity = 5 },
                new() { ItemCode = 2, OrderedQuantity = 3 },
            }
        };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    // ─── Delete ──────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_ValidCommand_PassesValidation()
    {
        var validator = new DeleteDeliveryChallanCommandValidator();
        var cmd = new DeleteDeliveryChallanCommand { ChallanCode = 1, CompanyCode = 1 };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Delete_MissingChallanCode_FailsValidation()
    {
        var validator = new DeleteDeliveryChallanCommandValidator();
        var cmd = new DeleteDeliveryChallanCommand { ChallanCode = 0, CompanyCode = 1 };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ChallanCode");
    }

    [Fact]
    public void Delete_MissingCompanyCode_FailsValidation()
    {
        var validator = new DeleteDeliveryChallanCommandValidator();
        var cmd = new DeleteDeliveryChallanCommand { ChallanCode = 1, CompanyCode = 0 };

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CompanyCode");
    }
}
