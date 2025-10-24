using ErpBE.Application.TaxInvoice.Commands;
using ErpBE.Application.TaxInvoice.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class DeleteTaxInvoiceCommandValidatorTests
    {
        private readonly DeleteTaxInvoiceCommandValidator _validator;

        public DeleteTaxInvoiceCommandValidatorTests()
        {
            _validator = new DeleteTaxInvoiceCommandValidator();
        }

        [Fact]
        public void Validate_WithValidData_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new DeleteTaxInvoiceCommand
            {
                InvoiceCode = 100,
                CompanyCode = 1
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        public void Validate_WithInvalidInvoiceCode_ShouldHaveValidationError(int invoiceCode)
        {
            // Arrange
            var command = new DeleteTaxInvoiceCommand
            {
                InvoiceCode = invoiceCode,
                CompanyCode = 1
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InvoiceCode)
                .WithErrorMessage("Invoice Code is required.");
        }

        [Fact]
        public void Validate_WithNegativeInvoiceCode_ShouldNotHaveValidationError()
        {
            // Arrange - Negative IDs are valid (SQL Server IDENTITY wrapping)
            var command = new DeleteTaxInvoiceCommand
            {
                InvoiceCode = -100,
                CompanyCode = 1
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.InvoiceCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidCompanyCode_ShouldHaveValidationError(int companyCode)
        {
            // Arrange
            var command = new DeleteTaxInvoiceCommand
            {
                InvoiceCode = 100,
                CompanyCode = companyCode
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CompanyCode)
                .WithErrorMessage("Company Code must be greater than 0.");
        }
    }
}

