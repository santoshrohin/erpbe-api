using ErpBE.Application.CustomerPo.Queries;
using FluentValidation;

namespace ErpBE.Application.CustomerPo.Validators;

/// <summary>
/// Validator for GetAllCustomerPosQuery
/// </summary>
public class GetAllCustomerPosQueryValidator : AbstractValidator<GetAllCustomerPosQuery>
{
    public GetAllCustomerPosQueryValidator()
    {
        RuleFor(x => x.Parameters.CompanyId)
            .GreaterThan(0)
            .WithMessage("Company ID is required.");

        RuleFor(x => x.Parameters.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page Number must be greater than 0.");

        RuleFor(x => x.Parameters.PageSize)
            .GreaterThan(0)
            .WithMessage("Page Size must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("Page Size cannot exceed 100.");

        RuleFor(x => x.Parameters.SortOrder)
            .Must(x => x.Equals("ASC", StringComparison.OrdinalIgnoreCase) || 
                      x.Equals("DESC", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort Order must be either ASC or DESC.");
    }
}

