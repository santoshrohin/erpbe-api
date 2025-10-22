using ErpBE.Domain.CommonDto;
using FluentValidation;

namespace ErpBE.Application.Common.Validators
{
    public class FarmerQueryParametersValidator : AbstractValidator<FarmerQueryParameters>
    {
        public FarmerQueryParametersValidator()
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
                .WithMessage("Invalid sort field. Valid fields are: FarmerId, FarmerName, FarmerCode, FarmerAddress, BranchId, LineId");

            RuleFor(x => x.BranchId)
                .GreaterThan(0).When(x => x.BranchId.HasValue)
                .WithMessage("Branch ID must be greater than 0");

            RuleFor(x => x.LineId)
                .GreaterThan(0).When(x => x.LineId.HasValue)
                .WithMessage("Line ID must be greater than 0");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(255).WithMessage("Search term cannot exceed 255 characters");

            RuleFor(x => x.FarmerCode)
                .MaximumLength(50).WithMessage("Farmer code cannot exceed 50 characters");

            RuleFor(x => x.FarmerName)
                .MaximumLength(100).WithMessage("Farmer name cannot exceed 100 characters");
        }

        private bool IsValidSortField(string field)
        {
            var validFields = new[] { "FarmerId", "FarmerName", "FarmerCode", "FarmerAddress", "BranchId", "LineId" };
            return validFields.Contains(field, StringComparer.OrdinalIgnoreCase);
        }
    }
}
