using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.CustomerMaster.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class DeleteCustomerMasterCommandValidatorTests
    {
        private readonly DeleteCustomerMasterCommandValidator _validator;

        public DeleteCustomerMasterCommandValidatorTests()
        {
            _validator = new DeleteCustomerMasterCommandValidator();
        }

        [Fact]
        public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
        {
            // Arrange
            var command = new DeleteCustomerMasterCommand
            {
                Id = 1,
                CompanyId = 1
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
            var command = new DeleteCustomerMasterCommand
            {
                Id = 0, // 0 is invalid
                CompanyId = 1
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
            var command = new DeleteCustomerMasterCommand
            {
                Id = 1,
                CompanyId = companyId
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CompanyId)
                .WithErrorMessage("Company ID must be greater than 0.");
        }

        [Fact]
        public void Validate_WithAllFieldsValid_ShouldNotHaveValidationErrors()
        {
            // Arrange
            var command = new DeleteCustomerMasterCommand
            {
                Id = 100,
                CompanyId = 5
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

