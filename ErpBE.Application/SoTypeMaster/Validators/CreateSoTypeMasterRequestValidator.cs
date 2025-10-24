using FluentValidation;
using ErpBE.Application.DTOs;

namespace ErpBE.Application.SoTypeMaster.Validators
{
    public class CreateSoTypeMasterRequestValidator : AbstractValidator<CreateSoTypeMasterRequest>
    {
        public CreateSoTypeMasterRequestValidator()
        {
            RuleFor(x => x.ShortName)
                .NotEmpty().WithMessage("Short Name is required.")
                .MaximumLength(50).WithMessage("Short Name cannot exceed 50 characters.")
                .Matches(@"^[a-zA-Z0-9\s\-_]+$").WithMessage("Short Name contains invalid characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(50).WithMessage("Description cannot exceed 50 characters.");

            RuleFor(x => x.FirstLetter)
                .NotEmpty().WithMessage("First Letter is required.")
                .MaximumLength(50).WithMessage("First Letter cannot exceed 50 characters.");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("Company ID is required.");
        }
    }
}

