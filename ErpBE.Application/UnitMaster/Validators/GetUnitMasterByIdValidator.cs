using FluentValidation;
using ErpBE.Application.Interfaces;
using ErpBE.Application.UnitMaster.Queries;
using ErpBE.Application.Common.Validators;

namespace ErpBE.Application.UnitMaster.Validators
{
    public class GetUnitMasterByIdValidator : AbstractValidator<GetUnitMasterByIdQuery>
    {
        private readonly IUnitMasterRepository _repository;

        public GetUnitMasterByIdValidator(IUnitMasterRepository repository)
        {
            _repository = repository;

            RuleFor(x => x.Id)
                .NotZero("Unit ID")
                .MustAsync(async (id, cancellation) => 
                    await _repository.GetUnitMasterByIdAsync(id) != null)
                .WithMessage(x => $"Unit master with ID '{x.Id}' not found.");
        }
    }
}
