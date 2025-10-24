using ErpBE.Application.Common.Models;
using ErpBE.Application.CustomerMaster.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class CustomerMasterQueryParametersValidatorTests
    {
        private readonly CustomerMasterQueryParametersValidator _validator;

        public CustomerMasterQueryParametersValidatorTests()
        {
            _validator = new CustomerMasterQueryParametersValidator();
        }

        [Fact]
        public void Validate_WithValidParameters_ShouldNotHaveValidationErrors()
        {
            // Arrange
            var parameters = new CustomerMasterQueryParameters
            {
                CompanyId = 1,
                PageNumber = 1,
                PageSize = 10,
                SortDirection = "asc"
            };

            // Act
            var result = _validator.TestValidate(parameters);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidCompanyId_ShouldHaveValidationError(int companyId)
        {
            // Arrange
            var parameters = new CustomerMasterQueryParameters
            {
                CompanyId = companyId,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = _validator.TestValidate(parameters);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CompanyId)
                .WithErrorMessage("Company ID must be greater than 0.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidPageNumber_ShouldHaveValidationError(int pageNumber)
        {
            // Arrange
            var parameters = new CustomerMasterQueryParameters
            {
                CompanyId = 1,
                PageNumber = pageNumber,
                PageSize = 10
            };

            // Act
            var result = _validator.TestValidate(parameters);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage("Page number must be greater than 0.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidPageSize_ShouldHaveValidationError(int pageSize)
        {
            // Arrange
            var parameters = new CustomerMasterQueryParameters
            {
                CompanyId = 1,
                PageNumber = 1,
                PageSize = pageSize
            };

            // Act
            var result = _validator.TestValidate(parameters);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage("Page size must be greater than 0.");
        }

        [Fact]
        public void Validate_WithPageSizeExceedingMaximum_ShouldHaveValidationError()
        {
            // Arrange
            var parameters = new CustomerMasterQueryParameters
            {
                CompanyId = 1,
                PageNumber = 1,
                PageSize = 101 // Exceeds maximum of 100
            };

            // Act
            var result = _validator.TestValidate(parameters);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage("Page size cannot exceed 100.");
        }

        [Theory]
        [InlineData("asc")]
        [InlineData("desc")]
        public void Validate_WithValidSortDirection_ShouldNotHaveValidationError(string sortDirection)
        {
            // Arrange
            var parameters = new CustomerMasterQueryParameters
            {
                CompanyId = 1,
                PageNumber = 1,
                PageSize = 10,
                SortDirection = sortDirection
            };

            // Act
            var result = _validator.TestValidate(parameters);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SortDirection);
        }

        [Theory]
        [InlineData("ascending")]
        [InlineData("descending")]
        [InlineData("ASC")]
        [InlineData("DESC")]
        [InlineData("invalid")]
        public void Validate_WithInvalidSortDirection_ShouldHaveValidationError(string sortDirection)
        {
            // Arrange
            var parameters = new CustomerMasterQueryParameters
            {
                CompanyId = 1,
                PageNumber = 1,
                PageSize = 10,
                SortDirection = sortDirection
            };

            // Act
            var result = _validator.TestValidate(parameters);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SortDirection)
                .WithErrorMessage("Sort direction must be 'asc' or 'desc'.");
        }

        [Fact]
        public void Validate_WithNullSortDirection_ShouldNotHaveValidationError()
        {
            // Arrange
            var parameters = new CustomerMasterQueryParameters
            {
                CompanyId = 1,
                PageNumber = 1,
                PageSize = 10,
                SortDirection = null
            };

            // Act
            var result = _validator.TestValidate(parameters);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SortDirection);
        }

        [Fact]
        public void Validate_WithEmptySortDirection_ShouldNotHaveValidationError()
        {
            // Arrange
            var parameters = new CustomerMasterQueryParameters
            {
                CompanyId = 1,
                PageNumber = 1,
                PageSize = 10,
                SortDirection = ""
            };

            // Act
            var result = _validator.TestValidate(parameters);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SortDirection);
        }

        [Fact]
        public void Validate_WithAllOptionalFilters_ShouldNotHaveValidationErrors()
        {
            // Arrange
            var parameters = new CustomerMasterQueryParameters
            {
                CompanyId = 1,
                IsActive = true,
                AreaCode = 1,
                CustomerType = "1",
                StateCode = 1,
                CityCode = 1,
                CategoryCode = 1,
                PageNumber = 1,
                PageSize = 50,
                SearchTerm = "test",
                SortBy = "PartyName",
                SortDirection = "desc"
            };

            // Act
            var result = _validator.TestValidate(parameters);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

