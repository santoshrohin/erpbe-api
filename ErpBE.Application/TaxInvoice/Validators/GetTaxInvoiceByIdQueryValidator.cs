using ErpBE.Application.TaxInvoice.Queries;
using FluentValidation;

namespace ErpBE.Application.TaxInvoice.Validators
{
    public class GetTaxInvoiceByIdQueryValidator : AbstractValidator<GetTaxInvoiceByIdQuery>
    {
        public GetTaxInvoiceByIdQueryValidator()
        {
            RuleFor(x => x.InvoiceCode)
                .NotEqual(0)
                .WithMessage("Invoice Code is required.");
            
            RuleFor(x => x.CompanyCode)
                .GreaterThan(0)
                .WithMessage("Company Code must be greater than 0.");
        }
    }
}

