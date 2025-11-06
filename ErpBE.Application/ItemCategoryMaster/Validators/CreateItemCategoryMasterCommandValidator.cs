using ErpBE.Application.ItemCategoryMaster.Commands;
using FluentValidation;

namespace ErpBE.Application.ItemCategoryMaster.Validators
{
    public class CreateItemCategoryMasterCommandValidator : AbstractValidator<CreateItemCategoryMasterCommand>
    {
        public CreateItemCategoryMasterCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull().WithMessage("Request is required.");

            When(x => x.Request != null, () =>
            {
                RuleFor(x => x.Request.CategoryName)
                    .NotEmpty().WithMessage("Item Category Name is required.")
                    .MaximumLength(50).WithMessage("Item Category Name cannot exceed 50 characters.");

                RuleFor(x => x.Request.CompanyId)
                    .GreaterThan(0).WithMessage("Company ID must be greater than zero.");
            });
        }
    }
}




