using FluentValidation;
using ErpBE.Domain.DTOs;
using ErpBE.Application.Common.Validators;

namespace ErpBE.Application.UnitMaster.Validators
{
    public class CreateUnitMasterValidator : AbstractValidator<CreateUnitMasterRequest>
    {
        public CreateUnitMasterValidator()
        {
            RuleFor(x => x.UnitName)
                .NotNullOrEmpty("Unit name")
                .MaxLength(10, "Unit name")
                .AlphanumericOnly("Unit name");

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
