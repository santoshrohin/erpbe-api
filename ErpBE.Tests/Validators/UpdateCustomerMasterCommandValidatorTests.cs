using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.CustomerMaster.Validators;
using ErpBE.Application.Interfaces;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class UpdateCustomerMasterCommandValidatorTests
    {
        private readonly Mock<ICustomerMasterRepository> _mockRepository;
        private readonly UpdateCustomerMasterCommandValidator _validator;

        public UpdateCustomerMasterCommandValidatorTests()
        {
            _mockRepository = new Mock<ICustomerMasterRepository>();
            _validator = new UpdateCustomerMasterCommandValidator(_mockRepository.Object);
        }

        [Fact]
        public async Task Should_Have_Error_When_Id_Is_Empty()
        {
            var command = new UpdateCustomerMasterCommand { Id = 0 };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Should_Have_Error_When_CompanyId_Is_Zero()
        {
            var command = new UpdateCustomerMasterCommand { CompanyId = 0 };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CompanyId);
        }

        [Fact]
        public async Task Should_Have_Error_When_PartyCode_Is_Empty()
        {
            var command = new UpdateCustomerMasterCommand { PartyCode = "" };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.PartyCode);
        }

        [Fact]
        public async Task Should_Have_Error_When_PartyName_Is_Empty()
        {
            var command = new UpdateCustomerMasterCommand { PartyName = "" };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.PartyName);
        }

        [Fact]
        public async Task Should_Have_Error_When_AreaCode_Is_Zero()
        {
            var command = new UpdateCustomerMasterCommand { AreaCode = 0 };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.AreaCode);
        }

        [Fact]
        public async Task Should_Have_Error_When_CustomerType_Is_Zero()
        {
            var command = new UpdateCustomerMasterCommand { CustomerType = 0 };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CustomerType);
        }

        [Fact]
        public async Task Should_Have_Error_When_Email_Is_Invalid()
        {
            var command = new UpdateCustomerMasterCommand { Email = "invalid-email" };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public async Task Should_Have_Error_When_GstNo_Is_Empty_And_LbtApplicable_Is_True()
        {
            var command = new UpdateCustomerMasterCommand 
            { 
                IsLbtApplicable = true,
                GstNo = null
            };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GstNo);
        }

        [Fact]
        public async Task Should_Not_Have_Error_When_All_Required_Fields_Are_Valid()
        {
            var command = new UpdateCustomerMasterCommand
            {
                Id = 1,
                CompanyId = 1,
                PartyCode = "CUST001",
                PartyName = "Test Customer",
                AreaCode = 1,
                CustomerType = 1
            };

            var result = await _validator.TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
            result.ShouldNotHaveValidationErrorFor(x => x.CompanyId);
            result.ShouldNotHaveValidationErrorFor(x => x.PartyCode);
            result.ShouldNotHaveValidationErrorFor(x => x.PartyName);
            result.ShouldNotHaveValidationErrorFor(x => x.AreaCode);
            result.ShouldNotHaveValidationErrorFor(x => x.CustomerType);
        }
    }
}

