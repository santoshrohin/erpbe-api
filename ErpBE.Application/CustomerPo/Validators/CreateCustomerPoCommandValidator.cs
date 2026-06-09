using ErpBE.Application.CustomerPo.Commands;
using FluentValidation;

namespace ErpBE.Application.CustomerPo.Validators;

/// <summary>
/// Validator for CreateCustomerPoCommand
/// </summary>
public class CreateCustomerPoCommandValidator : AbstractValidator<CreateCustomerPoCommand>
{
    public CreateCustomerPoCommandValidator()
    {
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
        RuleFor(x => x)
            .Must(x => !x.CustomerPoDate.HasValue || x.PoDate >= x.CustomerPoDate.Value)
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
            .GreaterThan(0)
            .WithMessage("ProjectCode must be a valid project ID (null means no project; 0 is not a valid project ID).")
            .When(x => x.ProjectCode.HasValue);

        // Line Items
        RuleFor(x => x.Details)
            .NotEmpty()
            .WithMessage("At least one line item is required.");

        RuleForEach(x => x.Details).SetValidator(new CreateCustomerPoDetailCommandValidator());
    }
}

/// <summary>
/// Validator for CreateCustomerPoDetailCommand
/// </summary>
public class CreateCustomerPoDetailCommandValidator : AbstractValidator<CreateCustomerPoDetailCommand>
{
    public CreateCustomerPoDetailCommandValidator()
    {
        RuleFor(x => x.ItemCode)
            .NotEqual(0)
            .WithMessage("Item is required.");

        RuleFor(x => x.UomCode)
            .NotEqual(0)
            .WithMessage("Unit of Measurement is required.");

        RuleFor(x => x.OrderedQuantity)
            .GreaterThan(0)
            .WithMessage("Ordered Quantity must be greater than 0.");

        RuleFor(x => x.Rate)
            .GreaterThan(0)
            .WithMessage("Rate must be greater than 0.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount must be 0 or greater.");

        // Dispatch quantity validation
        RuleFor(x => x.DispatchedQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Dispatched Quantity cannot be negative.")
            .LessThanOrEqualTo(x => x.OrderedQuantity)
            .WithMessage("Dispatched Quantity cannot exceed Ordered Quantity.");

        // Customer Item Code/Name
        RuleFor(x => x.CustomerItemCode)
            .MaximumLength(4000)
            .WithMessage("Customer Item Code is too long.")
            .When(x => !string.IsNullOrEmpty(x.CustomerItemCode));

        RuleFor(x => x.CustomerItemName)
            .MaximumLength(4000)
            .WithMessage("Customer Item Name is too long.")
            .When(x => !string.IsNullOrEmpty(x.CustomerItemName));

        // Discount validation
        RuleFor(x => x.DiscountPercentage)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Discount Percentage must be 0 or greater.")
            .LessThanOrEqualTo(100)
            .WithMessage("Discount Percentage cannot exceed 100%.")
            .When(x => x.DiscountPercentage.HasValue);
    }
}

