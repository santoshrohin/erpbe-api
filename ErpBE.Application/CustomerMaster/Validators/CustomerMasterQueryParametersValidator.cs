using ErpBE.Application.Common.Models;
using FluentValidation;

namespace ErpBE.Application.CustomerMaster.Validators
{
    public class CustomerMasterQueryParametersValidator : AbstractValidator<CustomerMasterQueryParameters>
    {
        public CustomerMasterQueryParametersValidator()
        {
            RuleFor(x => x.CompanyId)
                .GreaterThan(0)
                .WithMessage("Company ID must be greater than 0.");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("Page size must be greater than 0.")
                .LessThanOrEqualTo(100)
                .WithMessage("Page size cannot exceed 100.");

            RuleFor(x => x.SortDirection)
                .Must(x => x == "asc" || x == "desc")
                .WithMessage("Sort direction must be 'asc' or 'desc'.")
                .When(x => !string.IsNullOrEmpty(x.SortDirection));
        }
    }
}

