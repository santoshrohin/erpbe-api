using ErpBE.Application.DTOs;
using ErpBE.Application.SoTypeMaster.Validators;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class UpdateSoTypeMasterRequestValidatorTests
    {
        private readonly UpdateSoTypeMasterRequestValidator _validator;

        public UpdateSoTypeMasterRequestValidatorTests()
        {
            _validator = new UpdateSoTypeMasterRequestValidator();
        }

        [Fact]
        public async Task Validate_WithValidData_ShouldPass()
        {
            // Arrange
            var request = new UpdateSoTypeMasterRequest
            {
                Id = 1,
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
        [InlineData(0)]
        public async Task Validate_WithInvalidId_ShouldFail(int id)
        {
            // Arrange
            var request = new UpdateSoTypeMasterRequest
            {
                Id = id,
                CompanyId = 1,
                ShortName = "TEST_SO",
                Description = "Test Description",
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Id");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Validate_WithEmptyShortName_ShouldFail(string shortName)
        {
            // Arrange
            var request = new UpdateSoTypeMasterRequest
            {
                Id = 1,
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
        public async Task Validate_WithAllFieldsEmpty_ShouldFailWithMultipleErrors()
        {
            // Arrange
            var request = new UpdateSoTypeMasterRequest
            {
                Id = 0,
                CompanyId = 0,
                ShortName = "",
                Description = "",
                FirstLetter = ""
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCountGreaterThan(1);
        }
    }
}

