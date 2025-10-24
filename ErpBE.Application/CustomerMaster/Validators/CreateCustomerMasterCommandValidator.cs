using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.Interfaces;
using FluentValidation;

namespace ErpBE.Application.CustomerMaster.Validators
{
    public class CreateCustomerMasterCommandValidator : AbstractValidator<CreateCustomerMasterCommand>
    {
        private readonly ICustomerMasterRepository _repository;

        public CreateCustomerMasterCommandValidator(ICustomerMasterRepository repository)
        {
            _repository = repository;

            RuleFor(x => x.CompanyId)
                .GreaterThan(0)
                .WithMessage("Company ID must be greater than 0.");

            // Required: Customer Name
            RuleFor(x => x.PartyName)
                .NotEmpty()
                .WithMessage("Customer Name is required.")
                .MaximumLength(500)
                .WithMessage("Customer Name cannot exceed 500 characters.");

            // Required: Area Code
            RuleFor(x => x.AreaCode)
                .GreaterThan(0)
                .WithMessage("Area is required.");

            // Required: Customer Type
            RuleFor(x => x.CustomerType)
                .GreaterThan(0)
                .WithMessage("Customer Type is required.");

            // Optional fields with max lengths
            RuleFor(x => x.ContactPerson)
                .MaximumLength(75)
                .WithMessage("Contact Person cannot exceed 75 characters.")
                .When(x => !string.IsNullOrEmpty(x.ContactPerson));

            RuleFor(x => x.Abbreviation)
                .MaximumLength(20)
                .WithMessage("Abbreviation cannot exceed 20 characters.")
                .When(x => !string.IsNullOrEmpty(x.Abbreviation));

            RuleFor(x => x.Address)
                .MaximumLength(500)
                .WithMessage("Address cannot exceed 500 characters.")
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

            RuleFor(x => x.Website)
                .MaximumLength(100)
                .WithMessage("Website cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.Website));

            RuleFor(x => x.FaxNo)
                .MaximumLength(50)
                .WithMessage("Fax No cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.FaxNo));

            RuleFor(x => x.PinCode)
                .MaximumLength(15)
                .WithMessage("Pin Code cannot exceed 15 characters.")
                .When(x => !string.IsNullOrEmpty(x.PinCode));

            RuleFor(x => x.VatTinNo)
                .MaximumLength(50)
                .WithMessage("VAT/TIN No cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.VatTinNo));

            RuleFor(x => x.CstNo)
                .MaximumLength(50)
                .WithMessage("CST No cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.CstNo));

            RuleFor(x => x.GstNo)
                .MaximumLength(50)
                .WithMessage("GST No cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.GstNo));

            RuleFor(x => x.PanNo)
                .MaximumLength(25)
                .WithMessage("PAN No cannot exceed 25 characters.")
                .When(x => !string.IsNullOrEmpty(x.PanNo));

            RuleFor(x => x.ServiceTaxNo)
                .MaximumLength(50)
                .WithMessage("Service Tax No cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.ServiceTaxNo));

            RuleFor(x => x.TallyName)
                .MaximumLength(200)
                .WithMessage("Tally Name cannot exceed 200 characters.")
                .When(x => !string.IsNullOrEmpty(x.TallyName));

            RuleFor(x => x.OpeningBalanceType)
                .MaximumLength(10)
                .WithMessage("Opening Balance Type cannot exceed 10 characters.")
                .When(x => !string.IsNullOrEmpty(x.OpeningBalanceType));

            RuleFor(x => x.BankName)
                .MaximumLength(100)
                .WithMessage("Bank Name cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.BankName));

            RuleFor(x => x.BankAccountNo)
                .MaximumLength(50)
                .WithMessage("Bank Account No cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.BankAccountNo));

            RuleFor(x => x.BankBranchName)
                .MaximumLength(100)
                .WithMessage("Bank Branch Name cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.BankBranchName));

            RuleFor(x => x.BankIfscCode)
                .MaximumLength(20)
                .WithMessage("Bank IFSC Code cannot exceed 20 characters.")
                .When(x => !string.IsNullOrEmpty(x.BankIfscCode));

            RuleFor(x => x.Remark)
                .MaximumLength(500)
                .WithMessage("Remark cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Remark));

            // Conditional: GST No required if LBT Applicable
            RuleFor(x => x.GstNo)
                .NotEmpty()
                .WithMessage("GST No is required when LBT is applicable.")
                .When(x => x.IsLbtApplicable == true);
        }
    }
}

