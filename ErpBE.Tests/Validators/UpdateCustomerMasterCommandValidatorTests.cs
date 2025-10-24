using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.CustomerMaster.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class UpdateCustomerMasterCommandValidatorTests
    {
        private readonly UpdateCustomerMasterCommandValidator _validator;

        public UpdateCustomerMasterCommandValidatorTests()
        {
            _validator = new UpdateCustomerMasterCommandValidator();
        }

        [Fact]
        public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
        {
            // Arrange
            var command = new UpdateCustomerMasterCommand
            {
                Id = 1,
                CompanyId = 1,
                PartyCode = 1,
                PartyName = "Test Customer",
                AreaCode = 1,
                CustomerType = "1",
                IsActive = true
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithInvalidId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdateCustomerMasterCommand
            {
                Id = 0, // 0 is invalid
                CompanyId = 1,
                PartyCode = 1,
                PartyName = "Test Customer",
                AreaCode = 1,
                CustomerType = "1"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("ID is required.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidCompanyId_ShouldHaveValidationError(int companyId)
        {
            // Arrange
            var command = new UpdateCustomerMasterCommand
            {
                Id = 1,
                CompanyId = companyId,
                PartyCode = 1,
                PartyName = "Test Customer",
                AreaCode = 1,
                CustomerType = "1"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CompanyId)
                .WithErrorMessage("Company ID must be greater than 0.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidPartyCode_ShouldHaveValidationError(int partyCode)
        {
            // Arrange
            var command = new UpdateCustomerMasterCommand
            {
                Id = 1,
                CompanyId = 1,
                PartyCode = partyCode,
                PartyName = "Test Customer",
                AreaCode = 1,
                CustomerType = "1"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PartyCode)
                .WithErrorMessage("Party Code must be greater than 0.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithEmptyPartyName_ShouldHaveValidationError(string partyName)
        {
            // Arrange
            var command = new UpdateCustomerMasterCommand
            {
                Id = 1,
                CompanyId = 1,
                PartyCode = 1,
                PartyName = partyName,
                AreaCode = 1,
                CustomerType = "1"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PartyName)
                .WithErrorMessage("Customer Name is required.");
        }

        [Fact]
        public void Validate_WithPartyNameExceedingMaxLength_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdateCustomerMasterCommand
            {
                Id = 1,
                CompanyId = 1,
                PartyCode = 1,
                PartyName = new string('A', 501), // 501 characters
                AreaCode = 1,
                CustomerType = "1"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PartyName)
                .WithErrorMessage("Customer Name cannot exceed 500 characters.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidAreaCode_ShouldHaveValidationError(int areaCode)
        {
            // Arrange
            var command = new UpdateCustomerMasterCommand
            {
                Id = 1,
                CompanyId = 1,
                PartyCode = 1,
                PartyName = "Test Customer",
                AreaCode = areaCode,
                CustomerType = "1"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AreaCode)
                .WithErrorMessage("Area is required.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithEmptyCustomerType_ShouldHaveValidationError(string customerType)
        {
            // Arrange
            var command = new UpdateCustomerMasterCommand
            {
                Id = 1,
                CompanyId = 1,
                PartyCode = 1,
                PartyName = "Test Customer",
                AreaCode = 1,
                CustomerType = customerType
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CustomerType)
                .WithErrorMessage("Customer Type is required.");
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("test@")]
        [InlineData("@test.com")]
        public void Validate_WithInvalidEmailFormat_ShouldHaveValidationError(string email)
        {
            // Arrange
            var command = new UpdateCustomerMasterCommand
            {
                Id = 1,
                CompanyId = 1,
                PartyCode = 1,
                PartyName = "Test Customer",
                Email = email,
                AreaCode = 1,
                CustomerType = "1"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage("Email is not in a valid format.");
        }

        [Fact]
        public void Validate_WithLbtApplicableButNoLbtNo_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdateCustomerMasterCommand
            {
                Id = 1,
                CompanyId = 1,
                PartyCode = 1,
                PartyName = "Test Customer",
                AreaCode = 1,
                CustomerType = "1",
                IsLbtApplicable = true,
                LbtNo = null
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.LbtNo)
                .WithErrorMessage("GST No is required when LBT is applicable.");
        }

        [Fact]
        public void Validate_WithLbtApplicableAndLbtNo_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateCustomerMasterCommand
            {
                Id = 1,
                CompanyId = 1,
                PartyCode = 1,
                PartyName = "Test Customer",
                AreaCode = 1,
                CustomerType = "1",
                IsLbtApplicable = true,
                LbtNo = "GST123456"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.LbtNo);
        }
    }
}

