using ErpBE.Application.DTOs;
using FluentValidation;

namespace ErpBE.Application.ItemCategoryMaster.Validators
{
    public class CreateItemCategoryMasterRequestValidator : AbstractValidator<CreateItemCategoryMasterRequest>
    {
        public CreateItemCategoryMasterRequestValidator()
        {
            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("Item Category Name is required.")
                .MaximumLength(50).WithMessage("Item Category Name cannot exceed 50 characters.")
                .Must(BeValidCategoryName).WithMessage("Item Category Name contains invalid characters.");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("Company ID must be greater than zero.");
        }

        private bool BeValidCategoryName(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return false;

            // Category name should not contain special characters except spaces, hyphens, and underscores
            return !System.Text.RegularExpressions.Regex.IsMatch(categoryName, @"[^a-zA-Z0-9\s\-_]");
        }
    }
}


