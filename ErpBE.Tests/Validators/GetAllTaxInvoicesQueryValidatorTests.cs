using ErpBE.Application.TaxInvoice.Queries;
using ErpBE.Application.TaxInvoice.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class GetAllTaxInvoicesQueryValidatorTests
    {
        private readonly GetAllTaxInvoicesQueryValidator _validator;

        public GetAllTaxInvoicesQueryValidatorTests()
        {
            _validator = new GetAllTaxInvoicesQueryValidator();
        }

        [Fact]
        public void Validate_WithValidData_ShouldNotHaveValidationError()
        {
            // Arrange
            var query = new GetAllTaxInvoicesQuery
            {
                CompanyId = 1,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidCompanyId_ShouldHaveValidationError(int companyId)
        {
            // Arrange
            var query = new GetAllTaxInvoicesQuery
            {
                CompanyId = companyId,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CompanyId)
                .WithErrorMessage("Company ID is required.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidPageNumber_ShouldHaveValidationError(int pageNumber)
        {
            // Arrange
            var query = new GetAllTaxInvoicesQuery
            {
                CompanyId = 1,
                PageNumber = pageNumber,
                PageSize = 10
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage("Page number must be greater than 0.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(101)]
        [InlineData(150)]
        public void Validate_WithInvalidPageSize_ShouldHaveValidationError(int pageSize)
        {
            // Arrange
            var query = new GetAllTaxInvoicesQuery
            {
                CompanyId = 1,
                PageNumber = 1,
                PageSize = pageSize
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage("Page size must be between 1 and 100.");
        }

        [Fact]
        public void Validate_WithInvalidDateRange_ShouldHaveValidationError()
        {
            // Arrange
            var query = new GetAllTaxInvoicesQuery
            {
                CompanyId = 1,
                PageNumber = 1,
                PageSize = 10,
                InvoiceDateFrom = DateTime.Now,
                InvoiceDateTo = DateTime.Now.AddDays(-5) // DateTo < DateFrom
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InvoiceDateTo)
                .WithErrorMessage("Invoice Date To must be greater than or equal to Invoice Date From.");
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("random")]
        [InlineData("ASCENDING")]
        public void Validate_WithInvalidSortOrder_ShouldHaveValidationError(string sortOrder)
        {
            // Arrange
            var query = new GetAllTaxInvoicesQuery
            {
                CompanyId = 1,
                PageNumber = 1,
                PageSize = 10,
                SortOrder = sortOrder
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SortOrder)
                .WithErrorMessage("Sort order must be either 'asc' or 'desc'.");
        }

        [Theory]
        [InlineData("asc")]
        [InlineData("desc")]
        public void Validate_WithValidSortOrder_ShouldNotHaveValidationError(string sortOrder)
        {
            // Arrange
            var query = new GetAllTaxInvoicesQuery
            {
                CompanyId = 1,
                PageNumber = 1,
                PageSize = 10,
                SortOrder = sortOrder
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SortOrder);
        }
    }
}

