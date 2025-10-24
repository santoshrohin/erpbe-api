using ErpBE.Application.CustomerMaster.Commands;
using FluentValidation;

namespace ErpBE.Application.CustomerMaster.Validators
{
    public class DeleteCustomerMasterCommandValidator : AbstractValidator<DeleteCustomerMasterCommand>
    {
        public DeleteCustomerMasterCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Customer ID is required.");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0)
                .WithMessage("Company ID must be greater than 0.");
        }
    }
}

