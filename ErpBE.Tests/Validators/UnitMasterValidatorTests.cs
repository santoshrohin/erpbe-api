using System.Threading.Tasks;
using ErpBE.Application.UnitMaster.Validators;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using FluentAssertions;
using FluentValidation.TestHelper;
using NSubstitute;
using Xunit;

namespace ErpBE.Tests.Validators
{
    /// <summary>
    /// Unit tests for UnitMaster validators
    /// Tests all validation rules and edge cases
    /// </summary>
    public class UnitMasterValidatorTests
    {
        private readonly IUnitMasterRepository _mockRepository;

        public UnitMasterValidatorTests()
        {
            _mockRepository = Substitute.For<IUnitMasterRepository>();
        }

        #region CreateUnitMasterValidator Tests

        [Fact]
        public async Task CreateValidator_WithValidData_ShouldNotHaveValidationErrors()
        {
            // Arrange
            var validator = new CreateUnitMasterValidator(_mockRepository);
            _mockRepository.IsUnitNameUniqueAsync(Arg.Any<string>(), Arg.Any<int>(), null)
                .Returns(true);

            var request = new CreateUnitMasterRequest
            {
                UnitName = "KG",
                UnitDescription = "Kilogram",
                CompanyId = 1,
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task CreateValidator_WithEmptyUnitName_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new CreateUnitMasterValidator(_mockRepository);
            var request = new CreateUnitMasterRequest
            {
                UnitName = "",
                UnitDescription = "Test",
                CompanyId = 1,
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitName)
                .WithErrorMessage("Unit name is required");
        }

        [Fact]
        public async Task CreateValidator_WithNullUnitName_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new CreateUnitMasterValidator(_mockRepository);
            var request = new CreateUnitMasterRequest
            {
                UnitName = null!,
                UnitDescription = "Test",
                CompanyId = 1,
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitName);
        }

        [Fact]
        public async Task CreateValidator_WithTooLongUnitName_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new CreateUnitMasterValidator(_mockRepository);
            var request = new CreateUnitMasterRequest
            {
                UnitName = "VERYLONGNAME", // More than 10 chars
                UnitDescription = "Test",
                CompanyId = 1,
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitName)
                .WithErrorMessage("Unit name cannot exceed 10 characters");
        }

        [Fact]
        public async Task CreateValidator_WithSpecialCharactersInUnitName_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new CreateUnitMasterValidator(_mockRepository);
            var request = new CreateUnitMasterRequest
            {
                UnitName = "KG@#$",
                UnitDescription = "Test",
                CompanyId = 1,
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitName)
                .WithErrorMessage("Unit name can only contain letters, numbers, and spaces");
        }

        [Fact]
        public async Task CreateValidator_WithDuplicateUnitName_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new CreateUnitMasterValidator(_mockRepository);
            _mockRepository.IsUnitNameUniqueAsync("KG", 1, null)
                .Returns(false); // Name already exists

            var request = new CreateUnitMasterRequest
            {
                UnitName = "KG",
                UnitDescription = "Test",
                CompanyId = 1,
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitName)
                .WithErrorMessage("Unit with name 'KG' already exists for this company.");
        }

        [Fact]
        public async Task CreateValidator_WithTooLongDescription_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new CreateUnitMasterValidator(_mockRepository);
            _mockRepository.IsUnitNameUniqueAsync(Arg.Any<string>(), Arg.Any<int>(), null)
                .Returns(true);

            var request = new CreateUnitMasterRequest
            {
                UnitName = "KG",
                UnitDescription = new string('A', 101), // More than 100 chars
                CompanyId = 1,
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitDescription)
                .WithErrorMessage("Unit description cannot exceed 100 characters");
        }

        [Fact]
        public async Task CreateValidator_WithZeroCompanyId_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new CreateUnitMasterValidator(_mockRepository);
            var request = new CreateUnitMasterRequest
            {
                UnitName = "KG",
                UnitDescription = "Test",
                CompanyId = 0,
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CompanyId)
                .WithErrorMessage("Company ID must be greater than 0");
        }

        [Fact]
        public async Task CreateValidator_WithNegativeCompanyId_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new CreateUnitMasterValidator(_mockRepository);
            var request = new CreateUnitMasterRequest
            {
                UnitName = "KG",
                UnitDescription = "Test",
                CompanyId = -1,
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CompanyId)
                .WithErrorMessage("Company ID must be greater than 0");
        }

        #endregion

        #region UpdateUnitMasterValidator Tests

        [Fact]
        public async Task UpdateValidator_WithValidData_ShouldNotHaveValidationErrors()
        {
            // Arrange
            var validator = new UpdateUnitMasterValidator(_mockRepository);
            _mockRepository.GetUnitMasterByIdAsync(1)
                .Returns(new UnitMasterDto { Id = 1, UnitName = "KG", CompanyId = 1 });
            _mockRepository.IsUnitNameUniqueAsync("GRAM", 1, 1)
                .Returns(true);

            var request = new UpdateUnitMasterRequest
            {
                Id = 1,
                UnitName = "GRAM",
                UnitDescription = "Gram",
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task UpdateValidator_WithZeroId_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new UpdateUnitMasterValidator(_mockRepository);
            var request = new UpdateUnitMasterRequest
            {
                Id = 0,
                UnitName = "KG",
                UnitDescription = "Test",
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("Unit ID cannot be 0");
        }

        [Fact]
        public async Task UpdateValidator_WithNonExistentId_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new UpdateUnitMasterValidator(_mockRepository);
            _mockRepository.GetUnitMasterByIdAsync(999)
                .Returns((UnitMasterDto?)null); // Unit not found

            var request = new UpdateUnitMasterRequest
            {
                Id = 999,
                UnitName = "KG",
                UnitDescription = "Test",
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("Unit with ID '999' not found.");
        }

        [Fact]
        public async Task UpdateValidator_WithDuplicateUnitName_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new UpdateUnitMasterValidator(_mockRepository);
            _mockRepository.GetUnitMasterByIdAsync(1)
                .Returns(new UnitMasterDto { Id = 1, UnitName = "KG", CompanyId = 1 });
            _mockRepository.IsUnitNameUniqueAsync("GRAM", 1, 1)
                .Returns(false); // Name already exists for another unit

            var request = new UpdateUnitMasterRequest
            {
                Id = 1,
                UnitName = "GRAM",
                UnitDescription = "Test",
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitName)
                .WithErrorMessage("Unit with name 'GRAM' already exists for this company.");
        }

        [Fact]
        public async Task UpdateValidator_WithEmptyUnitName_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new UpdateUnitMasterValidator(_mockRepository);
            var request = new UpdateUnitMasterRequest
            {
                Id = 1,
                UnitName = "",
                UnitDescription = "Test",
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitName)
                .WithErrorMessage("Unit name is required");
        }

        #endregion

        #region UnitMasterQueryValidator Tests

        [Fact]
        public async Task QueryValidator_WithValidParameters_ShouldNotHaveValidationErrors()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                CompanyId = 1,
                UnitName = "KG",
                SearchTerm = "test",
                SortBy = "UnitName",
                SortDirection = "ASC",
                IsActive = true
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task QueryValidator_WithZeroPageNumber_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = 0,
                PageSize = 10
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage("Page number must be greater than 0");
        }

        [Fact]
        public async Task QueryValidator_WithNegativePageNumber_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = -1,
                PageSize = 10
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        }

        [Fact]
        public async Task QueryValidator_WithZeroPageSize_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 0
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage("Page size must be greater than 0");
        }

        [Fact]
        public async Task QueryValidator_WithExcessivePageSize_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 150 // Exceeds max of 100
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage("Page size cannot exceed 100");
        }

        [Fact]
        public async Task QueryValidator_WithZeroCompanyId_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                CompanyId = 0
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CompanyId)
                .WithErrorMessage("Company ID must be greater than 0");
        }

        [Fact]
        public async Task QueryValidator_WithNullCompanyId_ShouldPass()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                CompanyId = null
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.CompanyId);
        }

        [Fact]
        public async Task QueryValidator_WithTooLongUnitName_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                UnitName = "VeryLongName" // More than 10 chars
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitName)
                .WithErrorMessage("Unit name filter cannot exceed 10 characters");
        }

        [Fact]
        public async Task QueryValidator_WithSpecialCharsInUnitName_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                UnitName = "Test@#$"
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitName)
                .WithErrorMessage("Unit name filter can only contain letters, numbers, and spaces");
        }

        [Fact]
        public async Task QueryValidator_WithTooLongSearchTerm_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = new string('A', 256) // More than 255 chars
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SearchTerm)
                .WithErrorMessage("Search term cannot exceed 255 characters");
        }

        [Fact]
        public async Task QueryValidator_WithInvalidSortBy_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "InvalidField"
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SortBy)
                .WithErrorMessage("Sort field must be one of: Id, UnitName, UnitDescription, CompanyId, IsActive");
        }

        [Fact]
        public async Task QueryValidator_WithValidSortBy_ShouldPass()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "UnitName"
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
        }

        [Fact]
        public async Task QueryValidator_WithInvalidSortDirection_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                SortDirection = "INVALID"
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SortDirection)
                .WithErrorMessage("Sort direction must be 'ASC' or 'DESC'");
        }

        [Fact]
        public async Task QueryValidator_WithValidSortDirection_ShouldPass()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                SortDirection = "DESC"
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SortDirection);
        }

        [Fact]
        public async Task QueryValidator_WithMinimalValidData_ShouldPass()
        {
            // Arrange
            var validator = new UnitMasterQueryValidator();
            var query = new UnitMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await validator.TestValidateAsync(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion
    }
}

