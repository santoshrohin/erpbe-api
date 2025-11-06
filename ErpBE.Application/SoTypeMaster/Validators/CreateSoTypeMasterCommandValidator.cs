using ErpBE.Application.SoTypeMaster.Commands;
using FluentValidation;

namespace ErpBE.Application.SoTypeMaster.Validators
{
    public class CreateSoTypeMasterCommandValidator : AbstractValidator<CreateSoTypeMasterCommand>
    {
        public CreateSoTypeMasterCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithMessage("Request is required.");

            When(x => x.Request != null, () =>
            {
                RuleFor(x => x.Request!.ShortName)
                    .NotEmpty().WithMessage("Short Name is required.")
                    .MaximumLength(50).WithMessage("Short Name cannot exceed 50 characters.")
                    .Matches(@"^[a-zA-Z0-9\s\-_]+$").WithMessage("Short Name contains invalid characters.");

                RuleFor(x => x.Request!.Description)
                    .NotEmpty().WithMessage("Description is required.")
                    .MaximumLength(50).WithMessage("Description cannot exceed 50 characters.");

                RuleFor(x => x.Request!.FirstLetter)
                    .NotEmpty().WithMessage("First Letter is required.")
                    .MaximumLength(50).WithMessage("First Letter cannot exceed 50 characters.");

                RuleFor(x => x.Request!.CompanyId)
                    .GreaterThan(0).WithMessage("Company ID is required.");
            });
        }
    }
}

