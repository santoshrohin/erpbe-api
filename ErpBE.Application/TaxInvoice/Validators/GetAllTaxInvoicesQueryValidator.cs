using ErpBE.Application.TaxInvoice.Queries;
using FluentValidation;

namespace ErpBE.Application.TaxInvoice.Validators
{
    public class GetAllTaxInvoicesQueryValidator : AbstractValidator<GetAllTaxInvoicesQuery>
    {
        public GetAllTaxInvoicesQueryValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEqual(0)
                .WithMessage("Company ID is required.");
            
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0.");
            
            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");
            
            When(x => x.InvoiceDateFrom.HasValue && x.InvoiceDateTo.HasValue, () =>
            {
                RuleFor(x => x.InvoiceDateTo)
                    .GreaterThanOrEqualTo(x => x.InvoiceDateFrom)
                    .WithMessage("Invoice Date To must be greater than or equal to Invoice Date From.");
            });
            
            RuleFor(x => x.SortOrder)
                .Must(order => order == "asc" || order == "desc")
                .WithMessage("Sort order must be either 'asc' or 'desc'.");
        }
    }
}

