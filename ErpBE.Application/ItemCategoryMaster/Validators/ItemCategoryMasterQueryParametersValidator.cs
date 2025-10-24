using ErpBE.Application.Common.Models;
using FluentValidation;

namespace ErpBE.Application.ItemCategoryMaster.Validators
{
    public class ItemCategoryMasterQueryParametersValidator : AbstractValidator<ItemCategoryMasterQueryParameters>
    {
        public ItemCategoryMasterQueryParametersValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than zero.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than zero.")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");

            RuleFor(x => x.SortDirection)
                .Must(dir => dir == "ASC" || dir == "DESC")
                .WithMessage("Sort direction must be either 'ASC' or 'DESC'.");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("Company ID must be greater than zero.")
                .When(x => x.CompanyId.HasValue);
        }
    }
}


