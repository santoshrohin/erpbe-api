using FluentValidation;
using ErpBE.Application.Interfaces;
using ErpBE.Application.UnitMaster.Queries;
using ErpBE.Application.Common.Validators;

namespace ErpBE.Application.UnitMaster.Validators
{
    public class GetUnitMasterByIdValidator : AbstractValidator<GetUnitMasterByIdQuery>
    {
        public GetUnitMasterByIdValidator()
        {
            // Only validate that ID is not zero
            // Existence check should be done by the handler, which can return null for non-existent IDs
            RuleFor(x => x.Id)
                .NotZero("Unit ID");
        }
    }
}
