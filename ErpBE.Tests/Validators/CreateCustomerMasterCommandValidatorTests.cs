using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.CustomerMaster.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class CreateCustomerMasterCommandValidatorTests
    {
        private readonly CreateCustomerMasterCommandValidator _validator;

        public CreateCustomerMasterCommandValidatorTests()
        {
            _validator = new CreateCustomerMasterCommandValidator();
        }

        [Fact]
        public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
        {
            // Arrange
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
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

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidCompanyId_ShouldHaveValidationError(int companyId)
        {
            // Arrange
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = companyId,
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
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithEmptyPartyName_ShouldHaveValidationError(string partyName)
        {
            // Arrange
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
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
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
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
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
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
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
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

        [Fact]
        public void Validate_WithCustomerTypeExceedingMaxLength_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
                PartyName = "Test Customer",
                AreaCode = 1,
                CustomerType = new string('1', 21) // 21 characters
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CustomerType)
                .WithErrorMessage("Customer Type cannot exceed 20 characters.");
        }

        [Fact]
        public void Validate_WithContactPersonExceedingMaxLength_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
                PartyName = "Test Customer",
                ContactPerson = new string('A', 76), // 76 characters
                AreaCode = 1,
                CustomerType = "1"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ContactPerson)
                .WithErrorMessage("Contact Person cannot exceed 75 characters.");
        }

        [Fact]
        public void Validate_WithAbbreviationExceedingMaxLength_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
                PartyName = "Test Customer",
                Abbreviation = new string('A', 21), // 21 characters
                AreaCode = 1,
                CustomerType = "1"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Abbreviation)
                .WithErrorMessage("Abbreviation cannot exceed 20 characters.");
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("test@")]
        [InlineData("@test.com")]
        public void Validate_WithInvalidEmailFormat_ShouldHaveValidationError(string email)
        {
            // Arrange
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
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
        public void Validate_WithValidEmail_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
                PartyName = "Test Customer",
                Email = "test@customer.com",
                AreaCode = 1,
                CustomerType = "1"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validate_WithEmailExceedingMaxLength_ShouldHaveValidationError()
        {
            // Arrange - Create an email with exactly 101 characters
            var longEmail = new string('a', 89) + "@example.com"; // 89 + 12 = 101 characters
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
                PartyName = "Test Customer",
                Email = longEmail,
                AreaCode = 1,
                CustomerType = "1"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage("Email cannot exceed 100 characters.");
        }

        [Fact]
        public void Validate_WithLbtApplicableButNoLbtNo_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
                PartyName = "Test Customer",
                AreaCode = 1,
                CustomerType = "1",
                IsLbtApplicable = true,
                LbtNo = null // Missing LBT No when LBT is applicable
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
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
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

        [Fact]
        public void Validate_WithLbtNotApplicableAndNoLbtNo_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
                PartyName = "Test Customer",
                AreaCode = 1,
                CustomerType = "1",
                IsLbtApplicable = false,
                LbtNo = null
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.LbtNo);
        }

        [Theory]
        [InlineData("Phone", 51)]
        [InlineData("Mobile", 56)]
        [InlineData("FaxNo", 51)]
        [InlineData("PinCode", 16)]
        [InlineData("PanNo", 26)]
        [InlineData("CstNo", 51)]
        [InlineData("VatNo", 51)]
        [InlineData("ServiceTaxNo", 51)]
        [InlineData("EccNo", 51)]
        [InlineData("LbtNo", 51)]
        [InlineData("ExciseRange", 51)]
        [InlineData("ExciseDivision", 51)]
        [InlineData("ExciseCollectorate", 51)]
        public void Validate_WithFieldExceedingMaxLength_ShouldHaveValidationError(string fieldName, int length)
        {
            // Arrange
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = 1,
                PartyName = "Test Customer",
                AreaCode = 1,
                CustomerType = "1"
            };

            var property = typeof(CreateCustomerMasterCommand).GetProperty(fieldName);
            property?.SetValue(command, new string('A', length));

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.Errors.Should().Contain(e => e.PropertyName == fieldName);
        }
    }
}

