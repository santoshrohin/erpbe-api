using FluentValidation;
using ErpBE.Application.Interfaces;
using ErpBE.Application.UnitMaster.Commands;
using ErpBE.Application.Common.Validators;

namespace ErpBE.Application.UnitMaster.Validators
{
    public class DeleteUnitMasterValidator : AbstractValidator<DeleteUnitMasterCommand>
    {
        private readonly IUnitMasterRepository _repository;

        public DeleteUnitMasterValidator(IUnitMasterRepository repository)
        {
            _repository = repository;

            RuleFor(x => x.Id)
                .NotZero("Unit ID")
                .MustAsync(async (id, cancellation) => 
                    await _repository.GetUnitMasterByIdAsync(id) != null)
                .WithMessage(x => $"Unit with ID '{x.Id}' not found.");
        }
    }
}

