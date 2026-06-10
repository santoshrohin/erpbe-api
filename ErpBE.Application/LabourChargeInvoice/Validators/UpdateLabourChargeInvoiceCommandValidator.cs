using ErpBE.Application.LabourChargeInvoice.Commands;
using FluentValidation;

namespace ErpBE.Application.LabourChargeInvoice.Validators;

public class UpdateLabourChargeInvoiceCommandValidator : AbstractValidator<UpdateLabourChargeInvoiceCommand>
{
    public UpdateLabourChargeInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceCode).NotEqual(0).WithMessage("Invoice code is required.");
        RuleFor(x => x.CompanyCode).GreaterThan(0).WithMessage("Company code is required.");
        RuleFor(x => x.CustomerCode).GreaterThan(0).WithMessage("Customer is required.");
        RuleFor(x => x.InvoiceDate).NotEmpty().WithMessage("Invoice date is required.");
        RuleFor(x => x.Details).NotEmpty().WithMessage("At least one detail line is required.");

        RuleForEach(x => x.Details).ChildRules(d =>
        {
            d.RuleFor(x => x.InvoiceQuantity).GreaterThan(0).WithMessage("Invoice quantity must be greater than zero.");
        });
    }
}
