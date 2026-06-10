using ErpBE.Application.DeliveryChallan.Commands;
using FluentValidation;

namespace ErpBE.Application.DeliveryChallan.Validators;

public class CreateDeliveryChallanCommandValidator : AbstractValidator<CreateDeliveryChallanCommand>
{
    public CreateDeliveryChallanCommandValidator()
    {
        RuleFor(x => x.CompanyCode).GreaterThan(0).WithMessage("Company code is required.");
        RuleFor(x => x.CustomerCode).NotNull().NotEqual(0).WithMessage("Customer is required.");
        RuleFor(x => x.ChallanDate).NotNull().WithMessage("Challan date is required.");
        RuleFor(x => x.Details).NotEmpty().WithMessage("At least one detail line is required.");

        RuleForEach(x => x.Details).ChildRules(d =>
        {
            d.RuleFor(x => x.ItemCode).NotNull().NotEqual(0).WithMessage("Item code is required.");
            d.RuleFor(x => x.OrderedQuantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });

        RuleFor(x => x.Details)
            .Must(details => details
                .Where(d => d.ItemCode.HasValue)
                .GroupBy(d => d.ItemCode)
                .All(g => g.Count() == 1))
            .WithMessage("Duplicate item codes are not allowed in the same challan.")
            .When(x => x.Details != null && x.Details.Count > 0);
    }
}
