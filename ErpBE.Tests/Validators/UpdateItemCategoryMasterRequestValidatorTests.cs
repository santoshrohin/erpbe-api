using ErpBE.Application.DTOs;
using ErpBE.Application.ItemCategoryMaster.Validators;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class UpdateItemCategoryMasterRequestValidatorTests
    {
        private readonly UpdateItemCategoryMasterRequestValidator _validator;

        public UpdateItemCategoryMasterRequestValidatorTests()
        {
            _validator = new UpdateItemCategoryMasterRequestValidator();
        }

        [Fact]
        public void Validate_WithValidRequest_ShouldPass()
        {
            // Arrange
            var request = new UpdateItemCategoryMasterRequest
            {
                CategoryId = 1,
                CategoryName = "ELECTRONICS",
                IsAutoShortClose = true,
                IsActive = true
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidCategoryId_ShouldFail(int categoryId)
        {
            // Arrange
            var request = new UpdateItemCategoryMasterRequest
            {
                CategoryId = categoryId,
                CategoryName = "ELECTRONICS"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CategoryId");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validate_WithEmptyCategoryName_ShouldFail(string categoryName)
        {
            // Arrange
            var request = new UpdateItemCategoryMasterRequest
            {
                CategoryId = 1,
                CategoryName = categoryName!
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
            var request = new UpdateItemCategoryMasterRequest
            {
                CategoryId = 1,
                CategoryName = new string('A', 51)
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
        public void Validate_WithInvalidCharacters_ShouldFail(string categoryName)
        {
            // Arrange
            var request = new UpdateItemCategoryMasterRequest
            {
                CategoryId = 1,
                CategoryName = categoryName
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CategoryName");
        }

        [Fact]
        public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
        {
            // Arrange
            var request = new UpdateItemCategoryMasterRequest
            {
                CategoryId = 0,
                CategoryName = ""
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
        }
    }
}



