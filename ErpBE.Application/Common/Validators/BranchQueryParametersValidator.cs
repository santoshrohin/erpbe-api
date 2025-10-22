using ErpBE.Domain.CommonDto;
using FluentValidation;

namespace ErpBE.Application.Common.Validators
{
    public class BranchQueryParametersValidator : AbstractValidator<BranchQueryParameters>
    {
        public BranchQueryParametersValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.SortDirection)
                .Must(x => string.IsNullOrEmpty(x) || x.ToLower() == "asc" || x.ToLower() == "desc")
                .WithMessage("Sort direction must be 'asc' or 'desc'");

            RuleFor(x => x.SortBy)
                .Must(x => string.IsNullOrEmpty(x) || IsValidSortField(x))
                .WithMessage("Invalid sort field. Valid fields are: BranchId, BranchName");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(255).WithMessage("Search term cannot exceed 255 characters");

            RuleFor(x => x.BranchName)
                .MaximumLength(100).WithMessage("Branch name cannot exceed 100 characters");
        }

        private bool IsValidSortField(string field)
        {
            var validFields = new[] { "BranchId", "BranchName" };
            return validFields.Contains(field, StringComparer.OrdinalIgnoreCase);
        }
    }
}
