using ErpBE.Application.DTOs;
using ErpBE.Application.SoTypeMaster.Validators;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class CreateSoTypeMasterRequestValidatorTests
    {
        private readonly CreateSoTypeMasterRequestValidator _validator;

        public CreateSoTypeMasterRequestValidatorTests()
        {
            _validator = new CreateSoTypeMasterRequestValidator();
        }

        [Fact]
        public async Task Validate_WithValidData_ShouldPass()
        {
            // Arrange
            var request = new CreateSoTypeMasterRequest
            {
                CompanyId = 1,
                ShortName = "TEST_SO",
                Description = "Test SO Type",
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Validate_WithEmptyShortName_ShouldFail(string shortName)
        {
            // Arrange
            var request = new CreateSoTypeMasterRequest
            {
                CompanyId = 1,
                ShortName = shortName,
                Description = "Test Description",
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ShortName");
        }

        [Fact]
        public async Task Validate_WithShortNameExceedingMaxLength_ShouldFail()
        {
            // Arrange
            var request = new CreateSoTypeMasterRequest
            {
                CompanyId = 1,
                ShortName = new string('A', 51), // 51 characters
                Description = "Test Description",
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ShortName" && 
                e.ErrorMessage.Contains("cannot exceed 50 characters"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Validate_WithEmptyDescription_ShouldFail(string description)
        {
            // Arrange
            var request = new CreateSoTypeMasterRequest
            {
                CompanyId = 1,
                ShortName = "TEST_SO",
                Description = description,
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Description");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Validate_WithEmptyFirstLetter_ShouldFail(string firstLetter)
        {
            // Arrange
            var request = new CreateSoTypeMasterRequest
            {
                CompanyId = 1,
                ShortName = "TEST_SO",
                Description = "Test Description",
                FirstLetter = firstLetter
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "FirstLetter");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_WithInvalidCompanyId_ShouldFail(int companyId)
        {
            // Arrange
            var request = new CreateSoTypeMasterRequest
            {
                CompanyId = companyId,
                ShortName = "TEST_SO",
                Description = "Test Description",
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CompanyId");
        }

        [Theory]
        [InlineData("Test@SO#Type")]
        [InlineData("Test<>SO")]
        public async Task Validate_WithInvalidCharactersInShortName_ShouldFail(string shortName)
        {
            // Arrange
            var request = new CreateSoTypeMasterRequest
            {
                CompanyId = 1,
                ShortName = shortName,
                Description = "Test Description",
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ShortName" && 
                e.ErrorMessage.Contains("invalid characters"));
        }
    }
}

