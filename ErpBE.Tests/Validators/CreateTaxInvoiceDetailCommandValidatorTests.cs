using ErpBE.Application.TaxInvoice.Commands;
using ErpBE.Application.TaxInvoice.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace ErpBE.Tests.Validators
{
    public class CreateTaxInvoiceDetailCommandValidatorTests
    {
        private readonly CreateTaxInvoiceDetailCommandValidator _validator;

        public CreateTaxInvoiceDetailCommandValidatorTests()
        {
            _validator = new CreateTaxInvoiceDetailCommandValidator();
        }

        #region Valid Cases

        [Fact]
        public void Validate_WithValidData_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = 1,
                InvoiceQuantity = 10,
                Rate = 100.50
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion

        #region Item Code Validation

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidItemCode_ShouldHaveValidationError(int itemCode)
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = itemCode,
                UomCode = 1,
                InvoiceQuantity = 10,
                Rate = 100
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ItemCode)
                .WithErrorMessage("Please select an item.");
        }

        #endregion

        #region UOM Validation

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidUomCode_ShouldHaveValidationError(int uomCode)
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = uomCode,
                InvoiceQuantity = 10,
                Rate = 100
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UomCode)
                .WithErrorMessage("Please select a unit of measurement.");
        }

        #endregion

        #region Quantity Validation

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Validate_WithInvalidQuantity_ShouldHaveValidationError(double quantity)
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = 1,
                InvoiceQuantity = quantity,
                Rate = 100
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InvoiceQuantity)
                .WithErrorMessage("Invoice quantity must be greater than 0.");
        }

        #endregion

        #region Rate Validation

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_WithInvalidRate_ShouldHaveValidationError(double rate)
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = 1,
                InvoiceQuantity = 10,
                Rate = rate
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Rate)
                .WithErrorMessage("Rate must be greater than 0.");
        }

        #endregion

        #region GST Percentage Validation

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        [InlineData(150)]
        public void Validate_WithInvalidCgstPercentage_ShouldHaveValidationError(double percentage)
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = 1,
                InvoiceQuantity = 10,
                Rate = 100,
                CgstPercentage = percentage
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CgstPercentage)
                .WithErrorMessage("CGST percentage must be between 0 and 100.");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_WithInvalidSgstPercentage_ShouldHaveValidationError(double percentage)
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = 1,
                InvoiceQuantity = 10,
                Rate = 100,
                SgstPercentage = percentage
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SgstPercentage)
                .WithErrorMessage("SGST percentage must be between 0 and 100.");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_WithInvalidIgstPercentage_ShouldHaveValidationError(double percentage)
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = 1,
                InvoiceQuantity = 10,
                Rate = 100,
                IgstPercentage = percentage
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.IgstPercentage)
                .WithErrorMessage("IGST percentage must be between 0 and 100.");
        }

        #endregion

        #region GST Mutual Exclusion Validation

        [Fact]
        public void Validate_WithBothCgstSgstAndIgst_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = 1,
                InvoiceQuantity = 10,
                Rate = 100,
                CgstPercentage = 9,
                SgstPercentage = 9,
                IgstPercentage = 18 // Cannot have both
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.IgstPercentage)
                .WithErrorMessage("Cannot have both CGST/SGST and IGST. Use CGST/SGST for Intra-State or IGST for Inter-State.");
        }

        [Fact]
        public void Validate_WithIgstAndCgst_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = 1,
                InvoiceQuantity = 10,
                Rate = 100,
                IgstPercentage = 18,
                CgstPercentage = 9 // Cannot have both
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CgstPercentage)
                .WithErrorMessage("Cannot have both IGST and CGST/SGST. Use CGST/SGST for Intra-State or IGST for Inter-State.");
        }

        [Fact]
        public void Validate_WithIgstAndSgst_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = 1,
                InvoiceQuantity = 10,
                Rate = 100,
                IgstPercentage = 18,
                SgstPercentage = 9 // Cannot have both
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SgstPercentage)
                .WithErrorMessage("Cannot have both IGST and CGST/SGST. Use CGST/SGST for Intra-State or IGST for Inter-State.");
        }

        [Fact]
        public void Validate_WithOnlyCgstAndSgst_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = 1,
                InvoiceQuantity = 10,
                Rate = 100,
                CgstPercentage = 9,
                SgstPercentage = 9,
                IgstPercentage = null // No IGST
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.IgstPercentage);
            result.ShouldNotHaveValidationErrorFor(x => x.CgstPercentage);
            result.ShouldNotHaveValidationErrorFor(x => x.SgstPercentage);
        }

        [Fact]
        public void Validate_WithOnlyIgst_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = 1,
                InvoiceQuantity = 10,
                Rate = 100,
                IgstPercentage = 18,
                CgstPercentage = null,
                SgstPercentage = null
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.IgstPercentage);
            result.ShouldNotHaveValidationErrorFor(x => x.CgstPercentage);
            result.ShouldNotHaveValidationErrorFor(x => x.SgstPercentage);
        }

        #endregion

        #region String Length Validation

        [Fact]
        public void Validate_WithExcessivelyLongRemarks_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = 1,
                InvoiceQuantity = 10,
                Rate = 100,
                Remarks = new string('A', 501) // > 500 characters
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Remarks)
                .WithErrorMessage("Remarks cannot exceed 500 characters.");
        }

        [Fact]
        public void Validate_WithExcessivelyLongSerialNumber_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = 1,
                InvoiceQuantity = 10,
                Rate = 100,
                SerialNumber = new string('A', 101) // > 100 characters
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SerialNumber)
                .WithErrorMessage("Serial number cannot exceed 100 characters.");
        }

        [Fact]
        public void Validate_WithExcessivelyLongBatchNumber_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateTaxInvoiceDetailCommand
            {
                ItemCode = 1,
                UomCode = 1,
                InvoiceQuantity = 10,
                Rate = 100,
                BatchNumber = new string('A', 51) // > 50 characters
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BatchNumber)
                .WithErrorMessage("Batch number cannot exceed 50 characters.");
        }

        #endregion
    }
}

