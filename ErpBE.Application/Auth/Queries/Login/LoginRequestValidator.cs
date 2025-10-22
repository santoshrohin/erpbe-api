using FluentValidation;

namespace ErpBE.Application.Auth.Queries.Login
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("CompanyId is required");

            RuleFor(x => x.FinancialYearCode)
                .NotEmpty().WithMessage("Financial Year Code is required");
        }
    }
}
