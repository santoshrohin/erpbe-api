using ErpBE.Application.CustomerMaster.Commands;
using FluentValidation;

namespace ErpBE.Application.CustomerMaster.Validators
{
    public class CreateCustomerMasterCommandValidator : AbstractValidator<CreateCustomerMasterCommand>
    {
        public CreateCustomerMasterCommandValidator()
        {
            RuleFor(x => x.CompanyId)
                .GreaterThan(0)
                .WithMessage("Company ID must be greater than 0.");

            RuleFor(x => x.PartyName)
                .NotEmpty()
                .WithMessage("Customer Name is required.")
                .MaximumLength(500)
                .WithMessage("Customer Name cannot exceed 500 characters.");

            RuleFor(x => x.AreaCode)
                .GreaterThan(0)
                .WithMessage("Area is required.");

            RuleFor(x => x.CustomerType)
                .NotEmpty()
                .WithMessage("Customer Type is required.")
                .MaximumLength(20)
                .WithMessage("Customer Type cannot exceed 20 characters.");

            RuleFor(x => x.ContactPerson)
                .MaximumLength(75)
                .WithMessage("Contact Person cannot exceed 75 characters.")
                .When(x => !string.IsNullOrEmpty(x.ContactPerson));

            RuleFor(x => x.Abbreviation)
                .MaximumLength(20)
                .WithMessage("Abbreviation cannot exceed 20 characters.")
                .When(x => !string.IsNullOrEmpty(x.Abbreviation));

            RuleFor(x => x.VendorCode)
                .MaximumLength(30)
                .WithMessage("Vendor Code cannot exceed 30 characters.")
                .When(x => !string.IsNullOrEmpty(x.VendorCode));

            RuleFor(x => x.Address)
                .MaximumLength(255)
                .WithMessage("Address cannot exceed 255 characters.")
                .When(x => !string.IsNullOrEmpty(x.Address));

            RuleFor(x => x.Phone)
                .MaximumLength(50)
                .WithMessage("Phone cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.Mobile)
                .MaximumLength(55)
                .WithMessage("Mobile cannot exceed 55 characters.")
                .When(x => !string.IsNullOrEmpty(x.Mobile));

            RuleFor(x => x.Email)
                .MaximumLength(100)
                .WithMessage("Email cannot exceed 100 characters.")
                .EmailAddress()
                .WithMessage("Email is not in a valid format.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.FaxNo)
                .MaximumLength(50)
                .WithMessage("Fax No cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.FaxNo));

            RuleFor(x => x.PinCode)
                .MaximumLength(15)
                .WithMessage("Pin Code cannot exceed 15 characters.")
                .When(x => !string.IsNullOrEmpty(x.PinCode));

            RuleFor(x => x.PanNo)
                .MaximumLength(25)
                .WithMessage("PAN No cannot exceed 25 characters.")
                .When(x => !string.IsNullOrEmpty(x.PanNo));

            RuleFor(x => x.CstNo)
                .MaximumLength(50)
                .WithMessage("CST No cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.CstNo));

            RuleFor(x => x.VatNo)
                .MaximumLength(50)
                .WithMessage("VAT No cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.VatNo));

            RuleFor(x => x.ServiceTaxNo)
                .MaximumLength(50)
                .WithMessage("Service Tax No cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.ServiceTaxNo));

            RuleFor(x => x.EccNo)
                .MaximumLength(50)
                .WithMessage("ECC No cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.EccNo));

            RuleFor(x => x.LbtNo)
                .MaximumLength(50)
                .WithMessage("GST/LBT No cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.LbtNo));

            RuleFor(x => x.ExciseRange)
                .MaximumLength(50)
                .WithMessage("Excise Range cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.ExciseRange));

            RuleFor(x => x.ExciseDivision)
                .MaximumLength(50)
                .WithMessage("Excise Division cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.ExciseDivision));

            RuleFor(x => x.ExciseCollectorate)
                .MaximumLength(50)
                .WithMessage("Excise Collectorate cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.ExciseCollectorate));

            // Conditional: LBT Number required if LBT Applicable
            RuleFor(x => x.LbtNo)
                .NotEmpty()
                .WithMessage("GST No is required when LBT is applicable.")
                .When(x => x.IsLbtApplicable == true);
        }
    }
}

