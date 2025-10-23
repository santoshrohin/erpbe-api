using FluentValidation;
using ErpBE.Application.Interfaces;
using ErpBE.Application.DTOs;
using ErpBE.Application.Common.Validators;

namespace ErpBE.Application.UnitMaster.Validators
{
    public class CreateUnitMasterValidator : AbstractValidator<CreateUnitMasterRequest>
    {
        private readonly IUnitMasterRepository _repository;

        public CreateUnitMasterValidator(IUnitMasterRepository repository)
        {
            _repository = repository;

            RuleFor(x => x.UnitName)
                .NotNullOrEmpty("Unit name")
                .MaxLength(10, "Unit name")
                .AlphanumericOnly("Unit name")
                .MustAsync(async (request, unitName, cancellation) => 
                    await _repository.IsUnitNameUniqueAsync(unitName, request.CompanyId))
                .WithMessage(x => $"Unit with name '{x.UnitName}' already exists for this company.");

            RuleFor(x => x.UnitDescription)
                .MaxLength(100, "Unit description")
                .When(x => !string.IsNullOrEmpty(x.UnitDescription));

            RuleFor(x => x.CompanyId)
                .GreaterThanZero("Company ID");

            RuleFor(x => x.IsActive)
                .NotNull()
                .WithMessage("IsActive status is required");
        }
    }
}
