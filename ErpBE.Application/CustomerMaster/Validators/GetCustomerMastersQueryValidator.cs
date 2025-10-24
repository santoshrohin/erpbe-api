using ErpBE.Application.CustomerMaster.Queries;
using FluentValidation;

namespace ErpBE.Application.CustomerMaster.Validators
{
    public class GetCustomerMastersQueryValidator : AbstractValidator<GetCustomerMastersQuery>
    {
        public GetCustomerMastersQueryValidator()
        {
            RuleFor(x => x.Parameters.CompanyId)
                .GreaterThan(0)
                .WithMessage("Company ID must be greater than 0.");

            RuleFor(x => x.Parameters.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.Parameters.PageSize)
                .GreaterThan(0)
                .WithMessage("Page size must be greater than 0.")
                .LessThanOrEqualTo(100)
                .WithMessage("Page size cannot exceed 100.");
        }
    }
}

