using ErpBE.Application.CustomerPo.Commands;
using FluentValidation;

namespace ErpBE.Application.CustomerPo.Validators;

/// <summary>
/// Validator for DeleteCustomerPoCommand
/// </summary>
public class DeleteCustomerPoCommandValidator : AbstractValidator<DeleteCustomerPoCommand>
{
    public DeleteCustomerPoCommandValidator()
    {
        RuleFor(x => x.PoCode)
            .NotEqual(0)
            .WithMessage("PO Code is required.");

        RuleFor(x => x.CompanyId)
            .NotEqual(0)
            .WithMessage("Company ID is required.");
    }
}

