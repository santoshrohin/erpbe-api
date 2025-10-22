using FluentValidation;

namespace ErpBE.Application.Common.Validators
{
    public static class CommonValidationRules
    {
        /// <summary>
        /// Validates that a string is not null or empty
        /// </summary>
        public static IRuleBuilderOptions<T, string> NotNullOrEmpty<T>(this IRuleBuilder<T, string> ruleBuilder, string fieldName = "Field")
        {
            return ruleBuilder
                .NotEmpty()
                .WithMessage($"{fieldName} is required");
        }

        /// <summary>
        /// Validates maximum length for a string
        /// </summary>
        public static IRuleBuilderOptions<T, string> MaxLength<T>(this IRuleBuilder<T, string> ruleBuilder, int maxLength, string fieldName = "Field")
        {
            return ruleBuilder
                .MaximumLength(maxLength)
                .WithMessage($"{fieldName} cannot exceed {maxLength} characters");
        }

        /// <summary>
        /// Validates maximum length for a nullable string
        /// </summary>
        public static IRuleBuilderOptions<T, string?> MaxLengthNullable<T>(this IRuleBuilder<T, string?> ruleBuilder, int maxLength, string fieldName = "Field")
        {
            return ruleBuilder
                .MaximumLength(maxLength)
                .WithMessage($"{fieldName} cannot exceed {maxLength} characters");
        }

        /// <summary>
        /// Validates that a numeric value is greater than zero
        /// </summary>
        public static IRuleBuilderOptions<T, int> GreaterThanZero<T>(this IRuleBuilder<T, int> ruleBuilder, string fieldName = "Field")
        {
            return ruleBuilder
                .GreaterThan(0)
                .WithMessage($"{fieldName} must be greater than 0");
        }

        /// <summary>
        /// Validates that a nullable numeric value is greater than zero
        /// </summary>
        public static IRuleBuilderOptions<T, int?> GreaterThanZero<T>(this IRuleBuilder<T, int?> ruleBuilder, string fieldName = "Field")
        {
            return ruleBuilder
                .GreaterThan(0)
                .WithMessage($"{fieldName} must be greater than 0");
        }

        /// <summary>
        /// Validates that a numeric value is greater than or equal to zero
        /// </summary>
        public static IRuleBuilderOptions<T, int> GreaterThanOrEqualToZero<T>(this IRuleBuilder<T, int> ruleBuilder, string fieldName = "Field")
        {
            return ruleBuilder
                .GreaterThanOrEqualTo(0)
                .WithMessage($"{fieldName} must be greater than or equal to 0");
        }

        /// <summary>
        /// Validates alphanumeric pattern (letters, numbers, and spaces only)
        /// </summary>
        public static IRuleBuilderOptions<T, string> AlphanumericOnly<T>(this IRuleBuilder<T, string> ruleBuilder, string fieldName = "Field")
        {
            return ruleBuilder
                .Matches("^[a-zA-Z0-9\\s]+$")
                .WithMessage($"{fieldName} can only contain letters, numbers, and spaces");
        }

        /// <summary>
        /// Validates alphanumeric pattern for nullable string (letters, numbers, and spaces only)
        /// </summary>
        public static IRuleBuilderOptions<T, string?> AlphanumericOnlyNullable<T>(this IRuleBuilder<T, string?> ruleBuilder, string fieldName = "Field")
        {
            return ruleBuilder
                .Matches("^[a-zA-Z0-9\\s]+$")
                .WithMessage($"{fieldName} can only contain letters, numbers, and spaces");
        }

        /// <summary>
        /// Validates alphanumeric pattern with special characters allowed
        /// </summary>
        public static IRuleBuilderOptions<T, string> AlphanumericWithSpecialChars<T>(this IRuleBuilder<T, string> ruleBuilder, string fieldName = "Field")
        {
            return ruleBuilder
                .Matches("^[a-zA-Z0-9\\s\\-_.,@#$%&*()]+$")
                .WithMessage($"{fieldName} contains invalid characters");
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder, string fieldName = "Email")
        {
            return ruleBuilder
                .EmailAddress()
                .WithMessage($"{fieldName} must be a valid email address");
        }

        /// <summary>
        /// Validates pagination parameters
        /// </summary>
        public static IRuleBuilderOptions<T, int> ValidPageNumber<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");
        }

        /// <summary>
        /// Validates page size with reasonable limits
        /// </summary>
        public static IRuleBuilderOptions<T, int> ValidPageSize<T>(this IRuleBuilder<T, int> ruleBuilder, int maxPageSize = 100)
        {
            return ruleBuilder
                .GreaterThan(0)
                .WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(maxPageSize)
                .WithMessage($"Page size cannot exceed {maxPageSize}");
        }

        /// <summary>
        /// Validates sort direction
        /// </summary>
        public static IRuleBuilderOptions<T, string> ValidSortDirection<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must(direction => string.IsNullOrEmpty(direction) || 
                      direction.Equals("ASC", StringComparison.OrdinalIgnoreCase) ||
                      direction.Equals("DESC", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Sort direction must be 'ASC' or 'DESC'");
        }

        /// <summary>
        /// Validates sort direction for nullable string
        /// </summary>
        public static IRuleBuilderOptions<T, string?> ValidSortDirectionNullable<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Must(direction => string.IsNullOrEmpty(direction) || 
                      direction.Equals("ASC", StringComparison.OrdinalIgnoreCase) ||
                      direction.Equals("DESC", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Sort direction must be 'ASC' or 'DESC'");
        }

        /// <summary>
        /// Validates against a list of allowed values
        /// </summary>
        public static IRuleBuilderOptions<T, string> MustBeOneOf<T>(this IRuleBuilder<T, string> ruleBuilder, string[] allowedValues, string fieldName = "Field")
        {
            return ruleBuilder
                .Must(value => string.IsNullOrEmpty(value) || allowedValues.Contains(value, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"{fieldName} must be one of: {string.Join(", ", allowedValues)}");
        }

        /// <summary>
        /// Validates against a list of allowed values for nullable string
        /// </summary>
        public static IRuleBuilderOptions<T, string?> MustBeOneOfNullable<T>(this IRuleBuilder<T, string?> ruleBuilder, string[] allowedValues, string fieldName = "Field")
        {
            return ruleBuilder
                .Must(value => string.IsNullOrEmpty(value) || allowedValues.Contains(value, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"{fieldName} must be one of: {string.Join(", ", allowedValues)}");
        }
    }
}
