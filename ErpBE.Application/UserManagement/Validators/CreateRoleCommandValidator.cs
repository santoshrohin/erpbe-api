using ErpBE.Application.UserManagement.Commands;
using FluentValidation;

namespace ErpBE.Application.UserManagement.Validators
{
    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull().WithMessage("Request is required.");

            When(x => x.Request != null, () =>
            {
                RuleFor(x => x.Request.RoleName)
                    .NotEmpty().WithMessage("Role Name is required.")
                    .MaximumLength(100).WithMessage("Role Name cannot exceed 100 characters.");

                RuleFor(x => x.Request.Description)
                    .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
            });
        }
    }
}




