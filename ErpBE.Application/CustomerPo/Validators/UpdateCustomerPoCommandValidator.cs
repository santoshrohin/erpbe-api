using ErpBE.Application.CustomerPo.Commands;
using FluentValidation;

namespace ErpBE.Application.CustomerPo.Validators;

/// <summary>
/// Validator for UpdateCustomerPoCommand
/// </summary>
public class UpdateCustomerPoCommandValidator : AbstractValidator<UpdateCustomerPoCommand>
{
    public UpdateCustomerPoCommandValidator()
    {
        RuleFor(x => x.PoCode)
            .NotEqual(0)
            .WithMessage("PO Code is required.");

        // Core PO Information
        RuleFor(x => x.CustomerCode)
            .NotEqual(0)
            .WithMessage("Customer is required.");

        RuleFor(x => x.PoNumber)
            .NotEmpty()
            .WithMessage("PO Number is required.")
            .MaximumLength(100)
            .WithMessage("PO Number cannot exceed 100 characters.");

        RuleFor(x => x.PoType)
            .NotEqual(0)
            .WithMessage("PO Type is required.");

        RuleFor(x => x.PoDate)
            .NotEmpty()
            .WithMessage("PO Date is required.");

        // Legacy rule: PO Date must not be earlier than Customer PO Date.
        // CustomerPO.aspx.cs line 230: if (PoDate < CustPoDate) → "PO Date Should Not Greater than Entry Date"
        // Compare by date only (legacy uses date pickers with no time component)
        RuleFor(x => x)
            .Must(x => !x.CustomerPoDate.HasValue || x.PoDate.Date >= x.CustomerPoDate.Value.Date)
            .WithMessage("PO Date must not be earlier than Customer PO Date.")
            .When(x => x.CustomerPoDate.HasValue && x.PoDate != default);

        RuleFor(x => x.CompanyId)
            .NotEqual(0)
            .WithMessage("Company ID is required.");

        // Payment Terms
        RuleFor(x => x.PaymentTerms)
            .MaximumLength(260)
            .WithMessage("Payment Terms cannot exceed 260 characters.")
            .When(x => !string.IsNullOrEmpty(x.PaymentTerms));

        // Tax Information
        RuleFor(x => x.TaxPercentage)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Tax Percentage must be 0 or greater.")
            .When(x => x.TaxPercentage.HasValue);

        // Amount Fields
        RuleFor(x => x.BasicAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Basic Amount must be 0 or greater.")
            .When(x => x.BasicAmount.HasValue);

        RuleFor(x => x.DiscountPercentage)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Discount Percentage must be 0 or greater.")
            .LessThanOrEqualTo(100)
            .WithMessage("Discount Percentage cannot exceed 100%.")
            .When(x => x.DiscountPercentage.HasValue);

        RuleFor(x => x.GrandTotal)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Grand Total cannot be negative.")
            .When(x => x.GrandTotal.HasValue);

        RuleFor(x => x.ProjectCode)
            .NotEqual(0)
            .WithMessage("ProjectCode must be a valid project ID (null means no project; 0 is not a valid project ID).")
            .When(x => x.ProjectCode.HasValue);

        // Line Items
        RuleFor(x => x.Details)
            .NotEmpty()
            .WithMessage("At least one line item is required.");

        RuleForEach(x => x.Details).SetValidator(new CreateCustomerPoDetailCommandValidator());
    }
}

