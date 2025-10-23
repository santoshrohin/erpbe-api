using FluentValidation;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using ErpBE.Application.Common.Validators;

namespace ErpBE.Application.UnitMaster.Validators
{
    public class UpdateUnitMasterValidator : AbstractValidator<UpdateUnitMasterRequest>
    {
        private readonly IUnitMasterRepository _repository;

        public UpdateUnitMasterValidator(IUnitMasterRepository repository)
        {
            _repository = repository;

            RuleFor(x => x.Id)
                .NotZero("Unit ID")
                .MustAsync(async (id, cancellation) => 
                    await _repository.GetUnitMasterByIdAsync(id) != null)
                .WithMessage(x => $"Unit with ID '{x.Id}' not found.");

            RuleFor(x => x.UnitName)
                .NotNullOrEmpty("Unit name")
                .MaxLength(10, "Unit name")
                .AlphanumericOnly("Unit name")
                .MustAsync(async (request, unitName, cancellation) =>
                {
                    // Get existing unit to check company ID
                    var existingUnit = await _repository.GetUnitMasterByIdAsync(request.Id);
                    if (existingUnit == null) return false;
                    
                    // Check if unit name is unique (excluding current record)
                    return await _repository.IsUnitNameUniqueAsync(unitName, existingUnit.CompanyId, request.Id);
                })
                .WithMessage(x => $"Unit with name '{x.UnitName}' already exists for this company.");

            RuleFor(x => x.UnitDescription)
                .MaxLength(100, "Unit description")
                .When(x => !string.IsNullOrEmpty(x.UnitDescription));

            RuleFor(x => x.IsActive)
                .NotNull()
                .WithMessage("IsActive status is required");
        }
    }
}
