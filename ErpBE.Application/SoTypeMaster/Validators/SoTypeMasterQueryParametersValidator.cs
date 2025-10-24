using FluentValidation;
using ErpBE.Application.Common.Models;

namespace ErpBE.Application.SoTypeMaster.Validators
{
    public class SoTypeMasterQueryParametersValidator : AbstractValidator<SoTypeMasterQueryParameters>
    {
        public SoTypeMasterQueryParametersValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).When(x => x.CompanyId.HasValue).WithMessage("CompanyId must be greater than 0 if provided.");

            RuleFor(x => x.SortDirection)
                .Must(x => x == null || x == "ASC" || x == "DESC")
                .WithMessage("SortDirection must be either 'ASC' or 'DESC'.");
        }
    }
}

