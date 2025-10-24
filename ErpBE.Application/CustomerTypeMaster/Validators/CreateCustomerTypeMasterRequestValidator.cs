using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using FluentValidation;

namespace ErpBE.Application.CustomerTypeMaster.Validators
{
    public class CreateCustomerTypeMasterRequestValidator : AbstractValidator<CreateCustomerTypeMasterRequest>
    {
        private readonly ICustomerTypeMasterRepository _repository;

        public CreateCustomerTypeMasterRequestValidator(ICustomerTypeMasterRepository repository)
        {
            _repository = repository;

            RuleFor(x => x.CompanyId)
                .GreaterThan(0)
                .WithMessage("Company ID must be greater than 0.");

            RuleFor(x => x.TypeCode)
                .NotEmpty()
                .WithMessage("Type Code is required.")
                .MaximumLength(50)
                .WithMessage("Type Code cannot exceed 50 characters.")
                .MustAsync(async (model, typeCode, cancellation) =>
                {
                    return await _repository.IsTypeCodeUniqueAsync(typeCode, null, model.CompanyId, cancellation);
                })
                .WithMessage("Type Code already exists in the system.");

            RuleFor(x => x.TypeDescription)
                .NotEmpty()
                .WithMessage("Type Description is required.")
                .MaximumLength(150)
                .WithMessage("Type Description cannot exceed 150 characters.");

            RuleFor(x => x.FirstLetter)
                .NotEmpty()
                .WithMessage("First Letter is required.")
                .MaximumLength(50)
                .WithMessage("First Letter cannot exceed 50 characters.")
                .Must((model, firstLetter) =>
                {
                    if (string.IsNullOrWhiteSpace(model.TypeDescription) || string.IsNullOrWhiteSpace(firstLetter))
                        return true;
                    
                    return model.TypeDescription.TrimStart().StartsWith(firstLetter.TrimStart(),
                        StringComparison.OrdinalIgnoreCase);
                })
                .WithMessage("First Letter must match the first character of Type Description.");
        }
    }
}

