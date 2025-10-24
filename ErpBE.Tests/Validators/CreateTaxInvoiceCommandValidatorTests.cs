using ErpBE.Application.TaxInvoice.Commands;
using ErpBE.Application.TaxInvoice.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class CreateTaxInvoiceCommandValidatorTests
    {
        private readonly CreateTaxInvoiceCommandValidator _validator;

        public CreateTaxInvoiceCommandValidatorTests()
        {
            _validator = new CreateTaxInvoiceCommandValidator();
        }

        #region Valid Cases

        [Fact]
        public void Validate_WithValidData_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand
                    {
                        ItemCode = 1,
                        UomCode = 1,
                        InvoiceQuantity = 10,
                        Rate = 100.50
                    }
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion

        #region Company Validation

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidCompanyCode_ShouldHaveValidationError(int companyCode)
        {
            // Arrange
            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode = companyCode,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CompanyCode)
                .WithErrorMessage("Company is required.");
        }

        #endregion

        #region Invoice Date Validation

        [Fact]
        public void Validate_WithEmptyInvoiceDate_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = default(DateTime),
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InvoiceDate);
        }

        #endregion

        #region Customer Validation

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidCustomerCode_ShouldHaveValidationError(int customerCode)
        {
            // Arrange
            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = customerCode,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CustomerCode)
                .WithErrorMessage("Please select a customer.");
        }

        #endregion

        #region Customer PO Validation (MANDATORY)

        [Fact]
        public void Validate_WithoutCustomerPo_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = null, // Missing
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CustomerPoCode)
                .WithErrorMessage("Please select a Customer PO.");
        }

        #endregion

        #region Line Items Validation

        [Fact]
        public void Validate_WithoutLineItems_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>() // Empty
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InvoiceDetails)
                .WithErrorMessage("At least one invoice line item is required.");
        }

        #endregion

        #region Date Range Validation

        [Fact]
        public void Validate_WithInvalidDateRange_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                DateFrom = DateTime.Now,
                DateTo = DateTime.Now.AddDays(-5), // DateTo < DateFrom
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DateTo)
                .WithErrorMessage("Date To must be greater than or equal to Date From.");
        }

        #endregion

        #region Export Validation

        [Fact]
        public void Validate_ExportWithoutCurrency_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                ExportFlag = true,
                CurrencyCode = null, // Missing for export
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CurrencyCode)
                .WithErrorMessage("Currency is required for export invoices.");
        }

        [Fact]
        public void Validate_ExportWithoutCurrencyRate_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                ExportFlag = true,
                CurrencyCode = 1,
                CurrencyRate = null, // Missing for export
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CurrencyRate)
                .WithErrorMessage("Currency rate must be greater than 0 for export invoices.");
        }

        #endregion

        #region Percentage Validation

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        [InlineData(150)]
        public void Validate_WithInvalidDiscountPercentage_ShouldHaveValidationError(double percentage)
        {
            // Arrange
            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                DiscountPercentage = percentage,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DiscountPercentage)
                .WithErrorMessage("Discount percentage must be between 0 and 100.");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_WithInvalidTcsPercentage_ShouldHaveValidationError(double percentage)
        {
            // Arrange
            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                TcsPercentage = percentage,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TcsPercentage)
                .WithErrorMessage("TCS percentage must be between 0 and 100.");
        }

        #endregion

        #region String Length Validation

        [Fact]
        public void Validate_WithExcessivelyLongVehicleNumber_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                VehicleNumber = new string('A', 51), // > 50 characters
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.VehicleNumber)
                .WithErrorMessage("Vehicle number cannot exceed 50 characters.");
        }

        [Fact]
        public void Validate_WithExcessivelyLongRemarks_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                Remarks = new string('A', 501), // > 500 characters
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Remarks)
                .WithErrorMessage("Remarks cannot exceed 500 characters.");
        }

        #endregion
    }
}

