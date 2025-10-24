using ErpBE.Application.Common.Models;
using ErpBE.Application.ItemCategoryMaster.Validators;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class ItemCategoryMasterQueryParametersValidatorTests
    {
        private readonly ItemCategoryMasterQueryParametersValidator _validator;

        public ItemCategoryMasterQueryParametersValidatorTests()
        {
            _validator = new ItemCategoryMasterQueryParametersValidator();
        }

        [Fact]
        public void Validate_WithValidParameters_ShouldPass()
        {
            // Arrange
            var parameters = new ItemCategoryMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 15,
                SortDirection = "ASC",
                CompanyId = 1
            };

            // Act
            var result = _validator.Validate(parameters);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidPageNumber_ShouldFail(int pageNumber)
        {
            // Arrange
            var parameters = new ItemCategoryMasterQueryParameters
            {
                PageNumber = pageNumber,
                PageSize = 15
            };

            // Act
            var result = _validator.Validate(parameters);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PageNumber");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidPageSize_ShouldFail(int pageSize)
        {
            // Arrange
            var parameters = new ItemCategoryMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            // Act
            var result = _validator.Validate(parameters);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
        }

        [Fact]
        public void Validate_WithPageSizeExceeding100_ShouldFail()
        {
            // Arrange
            var parameters = new ItemCategoryMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 101
            };

            // Act
            var result = _validator.Validate(parameters);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PageSize" && e.ErrorMessage.Contains("100"));
        }

        [Theory]
        [InlineData("INVALID")]
        [InlineData("asc")]
        [InlineData("desc")]
        [InlineData("")]
        public void Validate_WithInvalidSortDirection_ShouldFail(string sortDirection)
        {
            // Arrange
            var parameters = new ItemCategoryMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 15,
                SortDirection = sortDirection
            };

            // Act
            var result = _validator.Validate(parameters);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "SortDirection");
        }

        [Theory]
        [InlineData("ASC")]
        [InlineData("DESC")]
        public void Validate_WithValidSortDirection_ShouldPass(string sortDirection)
        {
            // Arrange
            var parameters = new ItemCategoryMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 15,
                SortDirection = sortDirection
            };

            // Act
            var result = _validator.Validate(parameters);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidCompanyId_ShouldFail(int companyId)
        {
            // Arrange
            var parameters = new ItemCategoryMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 15,
                CompanyId = companyId
            };

            // Act
            var result = _validator.Validate(parameters);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CompanyId");
        }

        [Fact]
        public void Validate_WithNullCompanyId_ShouldPass()
        {
            // Arrange
            var parameters = new ItemCategoryMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 15,
                CompanyId = null
            };

            // Act
            var result = _validator.Validate(parameters);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}



