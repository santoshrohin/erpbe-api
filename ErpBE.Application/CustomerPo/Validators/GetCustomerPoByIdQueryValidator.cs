using ErpBE.Application.CustomerPo.Queries;
using FluentValidation;

namespace ErpBE.Application.CustomerPo.Validators;

/// <summary>
/// Validator for GetCustomerPoByIdQuery
/// </summary>
public class GetCustomerPoByIdQueryValidator : AbstractValidator<GetCustomerPoByIdQuery>
{
    public GetCustomerPoByIdQueryValidator()
    {
        RuleFor(x => x.PoCode)
            .NotEqual(0)
            .WithMessage("PO Code is required.");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0)
            .WithMessage("Company ID is required.");
    }
}

