using ErpBE.Application.DeliveryChallan.Commands;
using FluentValidation;

namespace ErpBE.Application.DeliveryChallan.Validators;

public class DeleteDeliveryChallanCommandValidator : AbstractValidator<DeleteDeliveryChallanCommand>
{
    public DeleteDeliveryChallanCommandValidator()
    {
        RuleFor(x => x.ChallanCode).NotEqual(0).WithMessage("Challan code is required.");
        RuleFor(x => x.CompanyCode).GreaterThan(0).WithMessage("Company code is required.");
    }
}
