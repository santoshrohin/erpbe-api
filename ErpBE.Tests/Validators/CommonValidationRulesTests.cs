using System.Threading.Tasks;
using ErpBE.Application.Common.Validators;
using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace ErpBE.Tests.Validators
{
    /// <summary>
    /// Unit tests for CommonValidationRules
    /// Tests all validation extension methods and edge cases
    /// </summary>
    public class CommonValidationRulesTests
    {
        #region Test Classes

        private class TestModel
        {
            public string StringField { get; set; } = string.Empty;
            public string? NullableStringField { get; set; }
            public int IntField { get; set; }
            public int? NullableIntField { get; set; }
        }

        private class StringValidator : AbstractValidator<TestModel>
        {
            public StringValidator()
            {
                RuleFor(x => x.StringField).NotNullOrEmpty("Test field");
            }
        }

        private class MaxLengthValidator : AbstractValidator<TestModel>
        {
            public MaxLengthValidator()
            {
                RuleFor(x => x.StringField).MaxLength(10, "Test field");
            }
        }

        private class MaxLengthNullableValidator : AbstractValidator<TestModel>
        {
            public MaxLengthNullableValidator()
            {
                RuleFor(x => x.NullableStringField).MaxLengthNullable(10, "Test field");
            }
        }

        private class NotZeroValidator : AbstractValidator<TestModel>
        {
            public NotZeroValidator()
            {
                RuleFor(x => x.IntField).NotZero("Test field");
            }
        }

        private class GreaterThanZeroValidator : AbstractValidator<TestModel>
        {
            public GreaterThanZeroValidator()
            {
                RuleFor(x => x.IntField).GreaterThanZero("Test field");
            }
        }

        private class GreaterThanZeroNullableValidator : AbstractValidator<TestModel>
        {
            public GreaterThanZeroNullableValidator()
            {
                RuleFor(x => x.NullableIntField).GreaterThanZero("Test field");
            }
        }

        private class GreaterThanOrEqualToZeroValidator : AbstractValidator<TestModel>
        {
            public GreaterThanOrEqualToZeroValidator()
            {
                RuleFor(x => x.IntField).GreaterThanOrEqualToZero("Test field");
            }
        }

        private class AlphanumericValidator : AbstractValidator<TestModel>
        {
            public AlphanumericValidator()
            {
                RuleFor(x => x.StringField).AlphanumericOnly("Test field");
            }
        }

        private class AlphanumericNullableValidator : AbstractValidator<TestModel>
        {
            public AlphanumericNullableValidator()
            {
                RuleFor(x => x.NullableStringField).AlphanumericOnlyNullable("Test field");
            }
        }

        private class AlphanumericWithSpecialCharsValidator : AbstractValidator<TestModel>
        {
            public AlphanumericWithSpecialCharsValidator()
            {
                RuleFor(x => x.StringField).AlphanumericWithSpecialChars("Test field");
            }
        }

        private class EmailValidator : AbstractValidator<TestModel>
        {
            public EmailValidator()
            {
                RuleFor(x => x.StringField).ValidEmail("Test email");
            }
        }

        private class PageNumberValidator : AbstractValidator<TestModel>
        {
            public PageNumberValidator()
            {
                RuleFor(x => x.IntField).ValidPageNumber();
            }
        }

        private class PageSizeValidator : AbstractValidator<TestModel>
        {
            public PageSizeValidator()
            {
                RuleFor(x => x.IntField).ValidPageSize(50);
            }
        }

        private class SortDirectionValidator : AbstractValidator<TestModel>
        {
            public SortDirectionValidator()
            {
                RuleFor(x => x.StringField).ValidSortDirection();
            }
        }

        private class SortDirectionNullableValidator : AbstractValidator<TestModel>
        {
            public SortDirectionNullableValidator()
            {
                RuleFor(x => x.NullableStringField).ValidSortDirectionNullable();
            }
        }

        private class MustBeOneOfValidator : AbstractValidator<TestModel>
        {
            public MustBeOneOfValidator()
            {
                RuleFor(x => x.StringField).MustBeOneOf(new[] { "Option1", "Option2", "Option3" }, "Test field");
            }
        }

        private class MustBeOneOfNullableValidator : AbstractValidator<TestModel>
        {
            public MustBeOneOfNullableValidator()
            {
                RuleFor(x => x.NullableStringField).MustBeOneOfNullable(new[] { "Option1", "Option2", "Option3" }, "Test field");
            }
        }

        #endregion

        #region NotNullOrEmpty Tests

        [Fact]
        public async Task NotNullOrEmpty_WithValidString_ShouldPass()
        {
            var validator = new StringValidator();
            var model = new TestModel { StringField = "Valid" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task NotNullOrEmpty_WithEmptyString_ShouldFail()
        {
            var validator = new StringValidator();
            var model = new TestModel { StringField = "" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.StringField)
                .WithErrorMessage("Test field is required");
        }

        #endregion

        #region MaxLength Tests

        [Fact]
        public async Task MaxLength_WithValidLength_ShouldPass()
        {
            var validator = new MaxLengthValidator();
            var model = new TestModel { StringField = "Valid" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task MaxLength_WithTooLongString_ShouldFail()
        {
            var validator = new MaxLengthValidator();
            var model = new TestModel { StringField = "ThisIsVeryLongString" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.StringField)
                .WithErrorMessage("Test field cannot exceed 10 characters");
        }

        [Fact]
        public async Task MaxLengthNullable_WithNull_ShouldPass()
        {
            var validator = new MaxLengthNullableValidator();
            var model = new TestModel { NullableStringField = null };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion

        #region NotZero Tests

        [Fact]
        public async Task NotZero_WithPositiveValue_ShouldPass()
        {
            var validator = new NotZeroValidator();
            var model = new TestModel { IntField = 5 };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task NotZero_WithNegativeValue_ShouldPass()
        {
            var validator = new NotZeroValidator();
            var model = new TestModel { IntField = -5 };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task NotZero_WithZero_ShouldFail()
        {
            var validator = new NotZeroValidator();
            var model = new TestModel { IntField = 0 };
            var result = await validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.IntField)
                .WithErrorMessage("Test field cannot be 0");
        }

        #endregion

        #region GreaterThanZero Tests

        [Fact]
        public async Task GreaterThanZero_WithPositiveValue_ShouldPass()
        {
            var validator = new GreaterThanZeroValidator();
            var model = new TestModel { IntField = 1 };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task GreaterThanZero_WithZero_ShouldFail()
        {
            var validator = new GreaterThanZeroValidator();
            var model = new TestModel { IntField = 0 };
            var result = await validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.IntField)
                .WithErrorMessage("Test field must be greater than 0");
        }

        [Fact]
        public async Task GreaterThanZero_WithNegativeValue_ShouldFail()
        {
            var validator = new GreaterThanZeroValidator();
            var model = new TestModel { IntField = -1 };
            var result = await validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.IntField);
        }

        [Fact]
        public async Task GreaterThanZeroNullable_WithNull_ShouldPass()
        {
            var validator = new GreaterThanZeroNullableValidator();
            var model = new TestModel { NullableIntField = null };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion

        #region GreaterThanOrEqualToZero Tests

        [Fact]
        public async Task GreaterThanOrEqualToZero_WithZero_ShouldPass()
        {
            var validator = new GreaterThanOrEqualToZeroValidator();
            var model = new TestModel { IntField = 0 };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task GreaterThanOrEqualToZero_WithPositiveValue_ShouldPass()
        {
            var validator = new GreaterThanOrEqualToZeroValidator();
            var model = new TestModel { IntField = 10 };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task GreaterThanOrEqualToZero_WithNegativeValue_ShouldFail()
        {
            var validator = new GreaterThanOrEqualToZeroValidator();
            var model = new TestModel { IntField = -1 };
            var result = await validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.IntField)
                .WithErrorMessage("Test field must be greater than or equal to 0");
        }

        #endregion

        #region AlphanumericOnly Tests

        [Fact]
        public async Task AlphanumericOnly_WithValidString_ShouldPass()
        {
            var validator = new AlphanumericValidator();
            var model = new TestModel { StringField = "Test123 ABC" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task AlphanumericOnly_WithSpecialCharacters_ShouldFail()
        {
            var validator = new AlphanumericValidator();
            var model = new TestModel { StringField = "Test@#$" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.StringField)
                .WithErrorMessage("Test field can only contain letters, numbers, and spaces");
        }

        [Fact]
        public async Task AlphanumericOnlyNullable_WithNull_ShouldPass()
        {
            var validator = new AlphanumericNullableValidator();
            var model = new TestModel { NullableStringField = null };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion

        #region AlphanumericWithSpecialChars Tests

        [Fact]
        public async Task AlphanumericWithSpecialChars_WithValidChars_ShouldPass()
        {
            var validator = new AlphanumericWithSpecialCharsValidator();
            var model = new TestModel { StringField = "Test-123_@#$%&*()" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task AlphanumericWithSpecialChars_WithInvalidChars_ShouldFail()
        {
            var validator = new AlphanumericWithSpecialCharsValidator();
            var model = new TestModel { StringField = "Test[]{}<>" }; // Brackets not allowed
            var result = await validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.StringField)
                .WithErrorMessage("Test field contains invalid characters");
        }

        #endregion

        #region ValidEmail Tests

        [Fact]
        public async Task ValidEmail_WithValidEmail_ShouldPass()
        {
            var validator = new EmailValidator();
            var model = new TestModel { StringField = "test@example.com" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ValidEmail_WithInvalidEmail_ShouldFail()
        {
            var validator = new EmailValidator();
            var model = new TestModel { StringField = "invalid-email" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.StringField)
                .WithErrorMessage("Test email must be a valid email address");
        }

        #endregion

        #region ValidPageNumber Tests

        [Fact]
        public async Task ValidPageNumber_WithPositiveValue_ShouldPass()
        {
            var validator = new PageNumberValidator();
            var model = new TestModel { IntField = 1 };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ValidPageNumber_WithZero_ShouldFail()
        {
            var validator = new PageNumberValidator();
            var model = new TestModel { IntField = 0 };
            var result = await validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.IntField)
                .WithErrorMessage("Page number must be greater than 0");
        }

        #endregion

        #region ValidPageSize Tests

        [Fact]
        public async Task ValidPageSize_WithValidSize_ShouldPass()
        {
            var validator = new PageSizeValidator();
            var model = new TestModel { IntField = 25 };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ValidPageSize_WithZero_ShouldFail()
        {
            var validator = new PageSizeValidator();
            var model = new TestModel { IntField = 0 };
            var result = await validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.IntField)
                .WithErrorMessage("Page size must be greater than 0");
        }

        [Fact]
        public async Task ValidPageSize_WithExcessiveSize_ShouldFail()
        {
            var validator = new PageSizeValidator();
            var model = new TestModel { IntField = 100 };
            var result = await validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.IntField)
                .WithErrorMessage("Page size cannot exceed 50");
        }

        #endregion

        #region ValidSortDirection Tests

        [Fact]
        public async Task ValidSortDirection_WithASC_ShouldPass()
        {
            var validator = new SortDirectionValidator();
            var model = new TestModel { StringField = "ASC" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ValidSortDirection_WithDESC_ShouldPass()
        {
            var validator = new SortDirectionValidator();
            var model = new TestModel { StringField = "DESC" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ValidSortDirection_WithLowerCase_ShouldPass()
        {
            var validator = new SortDirectionValidator();
            var model = new TestModel { StringField = "asc" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ValidSortDirection_WithInvalidValue_ShouldFail()
        {
            var validator = new SortDirectionValidator();
            var model = new TestModel { StringField = "INVALID" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.StringField)
                .WithErrorMessage("Sort direction must be 'ASC' or 'DESC'");
        }

        [Fact]
        public async Task ValidSortDirectionNullable_WithNull_ShouldPass()
        {
            var validator = new SortDirectionNullableValidator();
            var model = new TestModel { NullableStringField = null };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion

        #region MustBeOneOf Tests

        [Fact]
        public async Task MustBeOneOf_WithValidOption_ShouldPass()
        {
            var validator = new MustBeOneOfValidator();
            var model = new TestModel { StringField = "Option1" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task MustBeOneOf_WithCaseInsensitiveOption_ShouldPass()
        {
            var validator = new MustBeOneOfValidator();
            var model = new TestModel { StringField = "option2" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task MustBeOneOf_WithInvalidOption_ShouldFail()
        {
            var validator = new MustBeOneOfValidator();
            var model = new TestModel { StringField = "InvalidOption" };
            var result = await validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.StringField)
                .WithErrorMessage("Test field must be one of: Option1, Option2, Option3");
        }

        [Fact]
        public async Task MustBeOneOfNullable_WithNull_ShouldPass()
        {
            var validator = new MustBeOneOfNullableValidator();
            var model = new TestModel { NullableStringField = null };
            var result = await validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion
    }
}

