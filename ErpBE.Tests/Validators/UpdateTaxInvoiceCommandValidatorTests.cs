using ErpBE.Application.TaxInvoice.Commands;
using ErpBE.Application.TaxInvoice.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class UpdateTaxInvoiceCommandValidatorTests
    {
        private readonly UpdateTaxInvoiceCommandValidator _validator;

        public UpdateTaxInvoiceCommandValidatorTests()
        {
            _validator = new UpdateTaxInvoiceCommandValidator();
        }

        [Fact]
        public void Validate_WithValidData_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateTaxInvoiceCommand
            {
                InvoiceCode = 100,
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
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
            var command = new UpdateTaxInvoiceCommand
            {
                InvoiceCode = invoiceCode,
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
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
            var command = new UpdateTaxInvoiceCommand
            {
                InvoiceCode = -100,
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.InvoiceCode);
        }

        [Fact]
        public void Validate_WithoutLineItems_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdateTaxInvoiceCommand
            {
                InvoiceCode = 100,
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>() // Empty
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InvoiceDetails)
                .WithErrorMessage("At least one invoice line item is required.");
        }
    }
}

