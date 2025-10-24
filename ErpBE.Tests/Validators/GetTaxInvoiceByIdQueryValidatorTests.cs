using ErpBE.Application.TaxInvoice.Queries;
using ErpBE.Application.TaxInvoice.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class GetTaxInvoiceByIdQueryValidatorTests
    {
        private readonly GetTaxInvoiceByIdQueryValidator _validator;

        public GetTaxInvoiceByIdQueryValidatorTests()
        {
            _validator = new GetTaxInvoiceByIdQueryValidator();
        }

        [Fact]
        public void Validate_WithValidData_ShouldNotHaveValidationError()
        {
            // Arrange
            var query = new GetTaxInvoiceByIdQuery
            {
                InvoiceCode = 100,
                CompanyCode = 1
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        public void Validate_WithInvalidInvoiceCode_ShouldHaveValidationError(int invoiceCode)
        {
            // Arrange
            var query = new GetTaxInvoiceByIdQuery
            {
                InvoiceCode = invoiceCode,
                CompanyCode = 1
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InvoiceCode)
                .WithErrorMessage("Invoice Code is required.");
        }

        [Fact]
        public void Validate_WithNegativeInvoiceCode_ShouldNotHaveValidationError()
        {
            // Arrange - Negative IDs are valid (SQL Server IDENTITY wrapping)
            var query = new GetTaxInvoiceByIdQuery
            {
                InvoiceCode = -100,
                CompanyCode = 1
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.InvoiceCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidCompanyCode_ShouldHaveValidationError(int companyCode)
        {
            // Arrange
            var query = new GetTaxInvoiceByIdQuery
            {
                InvoiceCode = 100,
                CompanyCode = companyCode
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CompanyCode)
                .WithErrorMessage("Company Code must be greater than 0.");
        }
    }
}

