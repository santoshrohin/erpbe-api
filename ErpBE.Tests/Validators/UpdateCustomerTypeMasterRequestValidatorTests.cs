using ErpBE.Application.CustomerTypeMaster.Validators;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class UpdateCustomerTypeMasterRequestValidatorTests
    {
        private readonly Mock<ICustomerTypeMasterRepository> _mockRepository;
        private readonly UpdateCustomerTypeMasterRequestValidator _validator;

        public UpdateCustomerTypeMasterRequestValidatorTests()
        {
            _mockRepository = new Mock<ICustomerTypeMasterRepository>();
            _validator = new UpdateCustomerTypeMasterRequestValidator(_mockRepository.Object);
        }

        [Fact]
        public async Task Validate_WithValidRequest_ShouldPass()
        {
            // Arrange
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CustomerTypeMasterDto { Id = 1 });
            _mockRepository.Setup(x => x.IsTypeCodeUniqueAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var request = new UpdateCustomerTypeMasterRequest
            {
                Id = 1,
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
        public async Task Validate_WithNonExistentId_ShouldFail()
        {
            // Arrange
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CustomerTypeMasterDto?)null);

            var request = new UpdateCustomerTypeMasterRequest
            {
                Id = 999,
                CompanyId = 1,
                TypeCode = "TEST_CODE",
                TypeDescription = "Test Description",
                FirstLetter = "T"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Id");
        }

        [Fact]
        public async Task Validate_WithZeroCompanyId_ShouldFail()
        {
            // Arrange
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CustomerTypeMasterDto { Id = 1 });

            var request = new UpdateCustomerTypeMasterRequest
            {
                Id = 1,
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
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CustomerTypeMasterDto { Id = 1 });

            var request = new UpdateCustomerTypeMasterRequest
            {
                Id = 1,
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
        public async Task Validate_WithDuplicateTypeCode_ShouldFail()
        {
            // Arrange
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CustomerTypeMasterDto { Id = 1 });
            _mockRepository.Setup(x => x.IsTypeCodeUniqueAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var request = new UpdateCustomerTypeMasterRequest
            {
                Id = 1,
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
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CustomerTypeMasterDto { Id = 1 });

            var request = new UpdateCustomerTypeMasterRequest
            {
                Id = 1,
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
        public async Task Validate_WithMismatchedFirstLetter_ShouldFail()
        {
            // Arrange
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CustomerTypeMasterDto { Id = 1 });
            _mockRepository.Setup(x => x.IsTypeCodeUniqueAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var request = new UpdateCustomerTypeMasterRequest
            {
                Id = 1,
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
    }
}

