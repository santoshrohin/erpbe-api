using ErpBE.Application.CustomerTypeMaster.Commands;
using ErpBE.Application.Interfaces;
using FluentValidation;

namespace ErpBE.Application.CustomerTypeMaster.Validators
{
    public class DeleteCustomerTypeMasterCommandValidator : AbstractValidator<DeleteCustomerTypeMasterCommand>
    {
        private readonly ICustomerTypeMasterRepository _repository;

        public DeleteCustomerTypeMasterCommandValidator(ICustomerTypeMasterRepository repository)
        {
            _repository = repository;

            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Customer Type ID is required.");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0)
                .WithMessage("Company ID must be greater than 0.");
        }
    }
}

