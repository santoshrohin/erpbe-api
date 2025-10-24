using ErpBE.Application.CustomerTypeMaster.Validators;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class CreateCustomerTypeMasterRequestValidatorTests
    {
        private readonly Mock<ICustomerTypeMasterRepository> _mockRepository;
        private readonly CreateCustomerTypeMasterRequestValidator _validator;

        public CreateCustomerTypeMasterRequestValidatorTests()
        {
            _mockRepository = new Mock<ICustomerTypeMasterRepository>();
            _validator = new CreateCustomerTypeMasterRequestValidator(_mockRepository.Object);
        }

        [Fact]
        public async Task Validate_WithValidRequest_ShouldPass()
        {
            // Arrange
            _mockRepository.Setup(x => x.IsTypeCodeUniqueAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var request = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = "TEST_CODE",
                TypeDescription = "Test Description",
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Validate_WithZeroCompanyId_ShouldFail()
        {
            // Arrange
            var request = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 0,
                TypeCode = "TEST_CODE",
                TypeDescription = "Test Description",
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CompanyId");
        }

        [Fact]
        public async Task Validate_WithEmptyTypeCode_ShouldFail()
        {
            // Arrange
            var request = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = "",
                TypeDescription = "Test Description",
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "TypeCode");
        }

        [Fact]
        public async Task Validate_WithTooLongTypeCode_ShouldFail()
        {
            // Arrange
            var request = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = new string('A', 51),
                TypeDescription = "Test Description",
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "TypeCode");
        }

        [Fact]
        public async Task Validate_WithDuplicateTypeCode_ShouldFail()
        {
            // Arrange
            _mockRepository.Setup(x => x.IsTypeCodeUniqueAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var request = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = "DUPLICATE_CODE",
                TypeDescription = "Test Description",
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "TypeCode");
        }

        [Fact]
        public async Task Validate_WithEmptyTypeDescription_ShouldFail()
        {
            // Arrange
            var request = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = "TEST_CODE",
                TypeDescription = "",
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "TypeDescription");
        }

        [Fact]
        public async Task Validate_WithTooLongTypeDescription_ShouldFail()
        {
            // Arrange
            var request = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = "TEST_CODE",
                TypeDescription = new string('A', 151),
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "TypeDescription");
        }

        [Fact]
        public async Task Validate_WithEmptyFirstLetter_ShouldFail()
        {
            // Arrange
            var request = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = "TEST_CODE",
                TypeDescription = "Test Description",
                FirstLetter = ""
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "FirstLetter");
        }

        [Fact]
        public async Task Validate_WithMismatchedFirstLetter_ShouldFail()
        {
            // Arrange
            _mockRepository.Setup(x => x.IsTypeCodeUniqueAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var request = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = "TEST_CODE",
                TypeDescription = "Test Description",
                FirstLetter = "X"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "FirstLetter");
        }

        [Fact]
        public async Task Validate_WithCaseInsensitiveFirstLetterMatch_ShouldPass()
        {
            // Arrange
            _mockRepository.Setup(x => x.IsTypeCodeUniqueAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var request = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = "TEST_CODE",
                TypeDescription = "Test Description",
                FirstLetter = "t"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}

