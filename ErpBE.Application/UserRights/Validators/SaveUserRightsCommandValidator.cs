using ErpBE.Application.UserRights.Commands;
using FluentValidation;

namespace ErpBE.Application.UserRights.Validators;

public class SaveUserRightsCommandValidator : AbstractValidator<SaveUserRightsCommand>
{
    public SaveUserRightsCommandValidator()
    {
        RuleFor(x => x.UserCode).GreaterThan(0);
        RuleFor(x => x.Rights).NotNull();
        RuleForEach(x => x.Rights).ChildRules(r =>
        {
            r.RuleFor(x => x.ScreenCode).GreaterThan(0);
            r.RuleFor(x => x.Bitmask)
             .NotEmpty()
             .Length(7)
             .Matches("^[01]{7}$").WithMessage("Bitmask must be exactly 7 characters of '0' or '1'.");
        });
    }
}

public class CopyUserRightsCommandValidator : AbstractValidator<CopyUserRightsCommand>
{
    public CopyUserRightsCommandValidator()
    {
        RuleFor(x => x.FromUserCode).GreaterThan(0);
        RuleFor(x => x.ToUserCode).GreaterThan(0);
        RuleFor(x => x).Must(x => x.FromUserCode != x.ToUserCode)
            .WithMessage("Source and target user must be different.");
    }
}
