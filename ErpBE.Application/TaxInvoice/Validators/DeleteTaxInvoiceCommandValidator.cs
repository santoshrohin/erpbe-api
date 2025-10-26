using ErpBE.Application.TaxInvoice.Commands;
using FluentValidation;

namespace ErpBE.Application.TaxInvoice.Validators
{
    public class DeleteTaxInvoiceCommandValidator : AbstractValidator<DeleteTaxInvoiceCommand>
    {
        public DeleteTaxInvoiceCommandValidator()
        {
            RuleFor(x => x.InvoiceCode)
                .NotEqual(0)
                .WithMessage("Invoice Code is required.");
            
            RuleFor(x => x.CompanyCode)
                .NotEqual(0)
                .WithMessage("Company Code is required.");
        }
    }
}

