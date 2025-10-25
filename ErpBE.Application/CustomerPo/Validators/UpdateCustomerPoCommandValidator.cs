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

        RuleFor(x => x.CompanyId)
            .GreaterThan(0)
            .WithMessage("Company ID is required.");

        RuleFor(x => x.ProjectCode)
            .NotNull()
            .WithMessage("Project Code is required.")
            .NotEqual(0)
            .WithMessage("Project Code must be valid.");

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
            .GreaterThan(0)
            .WithMessage("Grand Total must be greater than 0.")
            .When(x => x.GrandTotal.HasValue);

        // Line Items
        RuleFor(x => x.Details)
            .NotEmpty()
            .WithMessage("At least one line item is required.");

        RuleForEach(x => x.Details).SetValidator(new CreateCustomerPoDetailCommandValidator());
    }
}

