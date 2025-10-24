using ErpBE.Application.DTOs;
using ErpBE.Application.ItemCategoryMaster.Validators;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class CreateItemCategoryMasterRequestValidatorTests
    {
        private readonly CreateItemCategoryMasterRequestValidator _validator;

        public CreateItemCategoryMasterRequestValidatorTests()
        {
            _validator = new CreateItemCategoryMasterRequestValidator();
        }

        [Fact]
        public void Validate_WithValidRequest_ShouldPass()
        {
            // Arrange
            var request = new CreateItemCategoryMasterRequest
            {
                CategoryName = "ELECTRONICS",
                CompanyId = 1,
                IsAutoShortClose = false,
                IsActive = true
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validate_WithEmptyCategoryName_ShouldFail(string categoryName)
        {
            // Arrange
            var request = new CreateItemCategoryMasterRequest
            {
                CategoryName = categoryName!,
                CompanyId = 1
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CategoryName");
        }

        [Fact]
        public void Validate_WithCategoryNameExceedingMaxLength_ShouldFail()
        {
            // Arrange
            var request = new CreateItemCategoryMasterRequest
            {
                CategoryName = new string('A', 51), // 51 characters
                CompanyId = 1
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CategoryName" && e.ErrorMessage.Contains("50"));
        }

        [Theory]
        [InlineData("CATEGORY@123")]
        [InlineData("CATEGORY#ABC")]
        [InlineData("CATEGORY$TEST")]
        [InlineData("CATEGORY%VALUE")]
        public void Validate_WithInvalidCharactersInCategoryName_ShouldFail(string categoryName)
        {
            // Arrange
            var request = new CreateItemCategoryMasterRequest
            {
                CategoryName = categoryName,
                CompanyId = 1
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CategoryName" && e.ErrorMessage.Contains("invalid"));
        }

        [Theory]
        [InlineData("ELECTRONICS")]
        [InlineData("ELECTRONICS-123")]
        [InlineData("ELECTRONICS_PARTS")]
        [InlineData("ELECTRONICS 2024")]
        public void Validate_WithValidCharactersInCategoryName_ShouldPass(string categoryName)
        {
            // Arrange
            var request = new CreateItemCategoryMasterRequest
            {
                CategoryName = categoryName,
                CompanyId = 1
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidCompanyId_ShouldFail(int companyId)
        {
            // Arrange
            var request = new CreateItemCategoryMasterRequest
            {
                CategoryName = "ELECTRONICS",
                CompanyId = companyId
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CompanyId");
        }

        [Fact]
        public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
        {
            // Arrange
            var request = new CreateItemCategoryMasterRequest
            {
                CategoryName = "",
                CompanyId = 0
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
        }
    }
}



