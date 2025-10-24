using ErpBE.Application.TaxInvoice.Commands;
using FluentValidation;

namespace ErpBE.Application.TaxInvoice.Validators
{
    /// <summary>
    /// Validator for CreateTaxInvoiceCommand - Implements ALL validations from legacy TaxInvoice.aspx.cs
    /// </summary>
    public class CreateTaxInvoiceCommandValidator : AbstractValidator<CreateTaxInvoiceCommand>
    {
        public CreateTaxInvoiceCommandValidator()
        {
            // A. Form-Level Validations
            RuleFor(x => x.CompanyCode)
                .GreaterThan(0)
                .WithMessage("Company is required.");
            
            RuleFor(x => x.InvoiceDate)
                .NotEmpty()
                .WithMessage("Invoice Date is required.")
                .Must(BeAValidDate)
                .WithMessage("Please enter a valid Invoice Date.");
            
            // B. Customer Selection Validations
            RuleFor(x => x.CustomerCode)
                .GreaterThan(0)
                .WithMessage("Please select a customer.");
            
            // C. PO Selection Validations (MANDATORY as per business rules)
            RuleFor(x => x.CustomerPoCode)
                .NotEmpty()
                .WithMessage("Please select a Customer PO.");
            
            // D. Line Items Validation
            RuleFor(x => x.InvoiceDetails)
                .NotEmpty()
                .WithMessage("At least one invoice line item is required.");
            
            // E. Item Insert Validations (for each line item)
            RuleForEach(x => x.InvoiceDetails)
                .SetValidator(new CreateTaxInvoiceDetailCommandValidator());
            
            // F. Date Range Validation (if DateFrom and DateTo are provided)
            When(x => x.DateFrom.HasValue && x.DateTo.HasValue, () =>
            {
                RuleFor(x => x.DateTo)
                    .GreaterThanOrEqualTo(x => x.DateFrom)
                    .WithMessage("Date To must be greater than or equal to Date From.");
            });
            
            // G. Export-Specific Validations
            When(x => x.ExportFlag == true, () =>
            {
                RuleFor(x => x.CurrencyCode)
                    .NotEmpty()
                    .WithMessage("Currency is required for export invoices.");
                
                RuleFor(x => x.CurrencyRate)
                    .NotNull()
                    .WithMessage("Currency rate must be greater than 0 for export invoices.")
                    .GreaterThan(0)
                    .WithMessage("Currency rate must be greater than 0 for export invoices.");
            });
            
            // H. Percentage Validations (0-100)
            When(x => x.DiscountPercentage.HasValue, () =>
            {
                RuleFor(x => x.DiscountPercentage)
                    .InclusiveBetween(0, 100)
                    .WithMessage("Discount percentage must be between 0 and 100.");
            });
            
            When(x => x.TcsPercentage.HasValue, () =>
            {
                RuleFor(x => x.TcsPercentage)
                    .InclusiveBetween(0, 100)
                    .WithMessage("TCS percentage must be between 0 and 100.");
            });
            
            When(x => x.ServicePercentage.HasValue, () =>
            {
                RuleFor(x => x.ServicePercentage)
                    .InclusiveBetween(0, 100)
                    .WithMessage("Service percentage must be between 0 and 100.");
            });
            
            When(x => x.ServiceEducationCessPercentage.HasValue, () =>
            {
                RuleFor(x => x.ServiceEducationCessPercentage)
                    .InclusiveBetween(0, 100)
                    .WithMessage("Service education cess percentage must be between 0 and 100.");
            });
            
            When(x => x.ServiceHigherEducationCessPercentage.HasValue, () =>
            {
                RuleFor(x => x.ServiceHigherEducationCessPercentage)
                    .InclusiveBetween(0, 100)
                    .WithMessage("Service higher education cess percentage must be between 0 and 100.");
            });
            
            // I. String Length Validations
            When(x => !string.IsNullOrEmpty(x.VehicleNumber), () =>
            {
                RuleFor(x => x.VehicleNumber)
                    .MaximumLength(50)
                    .WithMessage("Vehicle number cannot exceed 50 characters.");
            });
            
            When(x => !string.IsNullOrEmpty(x.LrNumber), () =>
            {
                RuleFor(x => x.LrNumber)
                    .MaximumLength(50)
                    .WithMessage("LR number cannot exceed 50 characters.");
            });
            
            When(x => !string.IsNullOrEmpty(x.Remarks), () =>
            {
                RuleFor(x => x.Remarks)
                    .MaximumLength(500)
                    .WithMessage("Remarks cannot exceed 500 characters.");
            });
        }
        
        private bool BeAValidDate(DateTime date)
        {
            return date != default(DateTime) && date.Year >= 1900 && date.Year <= 2100;
        }
    }
    
    /// <summary>
    /// Validator for individual line items in Tax Invoice
    /// </summary>
    public class CreateTaxInvoiceDetailCommandValidator : AbstractValidator<CreateTaxInvoiceDetailCommand>
    {
        public CreateTaxInvoiceDetailCommandValidator()
        {
            // 1. Item Code Validation
            RuleFor(x => x.ItemCode)
                .GreaterThan(0)
                .WithMessage("Please select an item.");
            
            // 2. UOM Validation
            RuleFor(x => x.UomCode)
                .GreaterThan(0)
                .WithMessage("Please select a unit of measurement.");
            
            // 3. Quantity Validation (CANNOT be 0 or negative)
            RuleFor(x => x.InvoiceQuantity)
                .GreaterThan(0)
                .WithMessage("Invoice quantity must be greater than 0.");
            
            // 4. Rate Validation (CANNOT be 0 or negative)
            RuleFor(x => x.Rate)
                .GreaterThan(0)
                .WithMessage("Rate must be greater than 0.");
            
            // 5. Tax Percentage Validations (0-100)
            When(x => x.CgstPercentage.HasValue, () =>
            {
                RuleFor(x => x.CgstPercentage)
                    .InclusiveBetween(0, 100)
                    .WithMessage("CGST percentage must be between 0 and 100.");
            });
            
            When(x => x.SgstPercentage.HasValue, () =>
            {
                RuleFor(x => x.SgstPercentage)
                    .InclusiveBetween(0, 100)
                    .WithMessage("SGST percentage must be between 0 and 100.");
            });
            
            When(x => x.IgstPercentage.HasValue, () =>
            {
                RuleFor(x => x.IgstPercentage)
                    .InclusiveBetween(0, 100)
                    .WithMessage("IGST percentage must be between 0 and 100.");
            });
            
            // 6. GST Validation: Either (CGST + SGST) OR IGST, NOT BOTH
            When(x => x.CgstPercentage.HasValue || x.SgstPercentage.HasValue, () =>
            {
                RuleFor(x => x.IgstPercentage)
                    .Must(igst => !igst.HasValue || igst == 0)
                    .WithMessage("Cannot have both CGST/SGST and IGST. Use CGST/SGST for Intra-State or IGST for Inter-State.");
            });
            
            When(x => x.IgstPercentage.HasValue && x.IgstPercentage > 0, () =>
            {
                RuleFor(x => x.CgstPercentage)
                    .Must(cgst => !cgst.HasValue || cgst == 0)
                    .WithMessage("Cannot have both IGST and CGST/SGST. Use CGST/SGST for Intra-State or IGST for Inter-State.");
                
                RuleFor(x => x.SgstPercentage)
                    .Must(sgst => !sgst.HasValue || sgst == 0)
                    .WithMessage("Cannot have both IGST and CGST/SGST. Use CGST/SGST for Intra-State or IGST for Inter-State.");
            });
            
            // 7. String Length Validations
            When(x => !string.IsNullOrEmpty(x.Remarks), () =>
            {
                RuleFor(x => x.Remarks)
                    .MaximumLength(500)
                    .WithMessage("Remarks cannot exceed 500 characters.");
            });
            
            When(x => !string.IsNullOrEmpty(x.SerialNumber), () =>
            {
                RuleFor(x => x.SerialNumber)
                    .MaximumLength(100)
                    .WithMessage("Serial number cannot exceed 100 characters.");
            });
            
            When(x => !string.IsNullOrEmpty(x.BatchNumber), () =>
            {
                RuleFor(x => x.BatchNumber)
                    .MaximumLength(50)
                    .WithMessage("Batch number cannot exceed 50 characters.");
            });
        }
    }
}

