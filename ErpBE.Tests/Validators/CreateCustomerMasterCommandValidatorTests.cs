using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.CustomerMaster.Validators;
using ErpBE.Application.Interfaces;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class CreateCustomerMasterCommandValidatorTests
    {
        private readonly Mock<ICustomerMasterRepository> _mockRepository;
        private readonly CreateCustomerMasterCommandValidator _validator;

        public CreateCustomerMasterCommandValidatorTests()
        {
            _mockRepository = new Mock<ICustomerMasterRepository>();
            _validator = new CreateCustomerMasterCommandValidator(_mockRepository.Object);
        }

        [Fact]
        public async Task Should_Have_Error_When_CompanyId_Is_Zero()
        {
            var command = new CreateCustomerMasterCommand { CompanyId = 0 };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CompanyId);
        }

        [Fact]
        public async Task Should_Have_Error_When_PartyName_Is_Empty()
        {
            var command = new CreateCustomerMasterCommand { PartyName = "" };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.PartyName);
        }

        [Fact]
        public async Task Should_Have_Error_When_PartyName_Exceeds_MaxLength()
        {
            var command = new CreateCustomerMasterCommand { PartyName = new string('A', 501) };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.PartyName);
        }

        [Fact]
        public async Task Should_Have_Error_When_AreaCode_Is_Zero()
        {
            var command = new CreateCustomerMasterCommand { AreaCode = 0 };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.AreaCode);
        }

        [Fact]
        public async Task Should_Have_Error_When_CustomerType_Is_Zero()
        {
            var command = new CreateCustomerMasterCommand { CustomerType = 0 };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CustomerType);
        }

        [Fact]
        public async Task Should_Have_Error_When_Email_Is_Invalid()
        {
            var command = new CreateCustomerMasterCommand { Email = "invalid-email" };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public async Task Should_Not_Have_Error_When_Email_Is_Valid()
        {
            var command = new CreateCustomerMasterCommand { Email = "test@example.com" };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public async Task Should_Have_Error_When_GstNo_Is_Empty_And_LbtApplicable_Is_True()
        {
            var command = new CreateCustomerMasterCommand 
            { 
                IsLbtApplicable = true,
                GstNo = null
            };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GstNo);
        }

        [Fact]
        public async Task Should_Not_Have_Error_When_GstNo_Is_Empty_And_LbtApplicable_Is_False()
        {
            var command = new CreateCustomerMasterCommand 
            { 
                IsLbtApplicable = false,
                GstNo = null
            };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.GstNo);
        }

        [Fact]
        public async Task Should_Have_Error_When_ContactPerson_Exceeds_MaxLength()
        {
            var command = new CreateCustomerMasterCommand { ContactPerson = new string('A', 76) };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ContactPerson);
        }

        [Fact]
        public async Task Should_Have_Error_When_Abbreviation_Exceeds_MaxLength()
        {
            var command = new CreateCustomerMasterCommand { Abbreviation = new string('A', 21) };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Abbreviation);
        }

        [Fact]
        public async Task Should_Have_Error_When_Phone_Exceeds_MaxLength()
        {
            var command = new CreateCustomerMasterCommand { Phone = new string('1', 51) };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Phone);
        }

        [Fact]
        public async Task Should_Have_Error_When_Mobile_Exceeds_MaxLength()
        {
            var command = new CreateCustomerMasterCommand { Mobile = new string('1', 56) };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Mobile);
        }

        [Fact]
        public async Task Should_Have_Error_When_PanNo_Exceeds_MaxLength()
        {
            var command = new CreateCustomerMasterCommand { PanNo = new string('A', 26) };
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.PanNo);
        }

        [Fact]
        public async Task Should_Not_Have_Error_When_All_Required_Fields_Are_Valid()
        {
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
                PartyName = "Test Customer",
                AreaCode = 1,
                CustomerType = 1
            };

            var result = await _validator.TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.CompanyId);
            result.ShouldNotHaveValidationErrorFor(x => x.PartyName);
            result.ShouldNotHaveValidationErrorFor(x => x.AreaCode);
            result.ShouldNotHaveValidationErrorFor(x => x.CustomerType);
        }
    }
}

