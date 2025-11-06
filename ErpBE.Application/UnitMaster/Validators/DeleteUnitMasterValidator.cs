using FluentValidation;
using ErpBE.Application.Interfaces;
using ErpBE.Application.UnitMaster.Commands;
using ErpBE.Application.Common.Validators;

namespace ErpBE.Application.UnitMaster.Validators
{
    public class DeleteUnitMasterValidator : AbstractValidator<DeleteUnitMasterCommand>
    {
        public DeleteUnitMasterValidator()
        {
            // Only validate that ID is not zero
            // Existence check should be done by the handler, which can return false for non-existent IDs
            RuleFor(x => x.Id)
                .NotZero("Unit ID");
        }
    }
}

