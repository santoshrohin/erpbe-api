using ErpBE.API.Common;
using ErpBE.Application.DTOs;
using ErpBE.Application.DTOs.TaxInvoice;
using ErpBE.Application.TaxInvoice.Commands;
using ErpBE.Application.TaxInvoice.Queries;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ErpBE.API.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaxInvoiceController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IPdfService _pdfService;
        private readonly ILogger<TaxInvoiceController> _logger;

        public TaxInvoiceController(IMediator mediator, IPdfService pdfService, ILogger<TaxInvoiceController> logger)
        {
            _mediator = mediator;
            _pdfService = pdfService;
            _logger = logger;
        }

        /// <summary>
        /// Get all Tax Invoices with filtering, searching, sorting, and pagination
        /// </summary>
        [HttpGet]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
        public async Task<ActionResult<TaxInvoicePagedResponse>> GetAllTaxInvoices([FromQuery] GetAllTaxInvoicesQuery query)
        {
            _logger.LogInformation("GET /api/TaxInvoice - Getting Tax Invoices for Company: {CompanyId}", query.CompanyId);
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get Tax Invoice by ID
        /// </summary>
        [HttpGet("{id:long}")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
        public async Task<IActionResult> GetTaxInvoiceById(long id, [FromQuery] int companyId)
        {
            _logger.LogInformation("GET /api/TaxInvoice/{Id} - Getting Tax Invoice: {InvoiceCode}, Company: {CompanyId}", id, id, companyId);

            var query = new GetTaxInvoiceByIdQuery
            {
                InvoiceCode = id,
                CompanyCode = companyId
            };

            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { message = $"Tax Invoice with ID '{id}' not found." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Create a new Tax Invoice
        /// </summary>
        [HttpPost]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.Add)]
        public async Task<IActionResult> CreateTaxInvoice([FromBody] CreateTaxInvoiceRequest request)
        {
            _logger.LogInformation("POST /api/TaxInvoice - Creating Tax Invoice for Company: {CompanyCode}, Customer: {CustomerCode}", 
                request.CompanyCode, request.CustomerCode);

            var command = MapRequestToCommand(request);
            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetTaxInvoiceById), 
                new { id = result.InvoiceCode, companyId = result.CompanyCode }, result);
        }

        /// <summary>
        /// Update an existing Tax Invoice
        /// </summary>
        [HttpPut("{id:long}")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.Edit)]
        public async Task<IActionResult> UpdateTaxInvoice(long id, [FromBody] UpdateTaxInvoiceRequest request)
        {
            if (id != request.InvoiceCode)
            {
                return BadRequest(new { message = "Invoice Code in URL does not match Invoice Code in request body." });
            }

            _logger.LogInformation("PUT /api/TaxInvoice/{Id} - Updating Tax Invoice: {InvoiceCode}", id, id);

            var command = MapUpdateRequestToCommand(request);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Delete a Tax Invoice (soft delete)
        /// </summary>
        [HttpDelete("{id:long}")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.Delete)]
        public async Task<IActionResult> DeleteTaxInvoice(long id, [FromQuery] int companyId)
        {
            _logger.LogInformation("DELETE /api/TaxInvoice/{Id} - Deleting Tax Invoice: {InvoiceCode}, Company: {CompanyId}", id, id, companyId);

            var command = new DeleteTaxInvoiceCommand
            {
                InvoiceCode = id,
                CompanyCode = companyId
            };

            bool result;
            try
            {
                result = await _mediator.Send(command);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }

            if (!result)
            {
                return NotFound(new { message = $"Tax Invoice with ID '{id}' not found or could not be deleted." });
            }

            return NoContent();
        }

        #region Lookup / Dropdown Endpoints

        [HttpGet("customers")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
        public async Task<IActionResult> GetCustomers([FromQuery] int companyId)
        {
            var result = await _mediator.Send(new GetTaxInvoiceCustomersQuery { CompanyCode = companyId });
            return Ok(result);
        }

        [HttpGet("items")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
        public async Task<IActionResult> GetItems([FromQuery] int customerCode, [FromQuery] int companyId)
        {
            var result = await _mediator.Send(new GetTaxInvoiceItemsByCustomerQuery { CustomerCode = customerCode, CompanyCode = companyId });
            return Ok(result);
        }

        [HttpGet("item-details/{itemCode}")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
        public async Task<IActionResult> GetItemDetails(int itemCode, [FromQuery] int companyId)
        {
            var result = await _mediator.Send(new GetTaxInvoiceItemDetailsQuery { ItemCode = itemCode, CompanyCode = companyId });
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("pos")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
        public async Task<IActionResult> GetPOs([FromQuery] int itemCode, [FromQuery] int customerCode, [FromQuery] int companyId, [FromQuery] int? invoiceCode)
        {
            var result = await _mediator.Send(new GetTaxInvoicePOsQuery { ItemCode = itemCode, CustomerCode = customerCode, CompanyCode = companyId, InvoiceCode = invoiceCode });
            return Ok(result);
        }

        [HttpGet("company-state")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
        public async Task<IActionResult> GetCompanyState([FromQuery] int companyId)
        {
            var result = await _mediator.Send(new GetCompanyStateQuery { CompanyCode = companyId });
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("sales-tax")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
        public async Task<IActionResult> GetSalesTax([FromQuery] int companyId)
        {
            var result = await _mediator.Send(new GetSalesTaxMasterQuery { CompanyCode = companyId });
            return Ok(result);
        }

        #endregion

        #region Helper Methods

        private CreateTaxInvoiceCommand MapRequestToCommand(CreateTaxInvoiceRequest request)
        {
            return new CreateTaxInvoiceCommand
            {
                CompanyCode = request.CompanyCode,
                InvoiceDate = request.InvoiceDate,
                CustomerCode = request.CustomerCode,
                CustomerPoCode = request.CustomerPoCode,
                InvoiceType = request.InvoiceType,
                Type = request.Type,
                PaymentMethodCode = request.PaymentMethodCode,
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
                IsSupplementary = request.IsSupplementary,
                ParentInvoiceCode = request.ParentInvoiceCode,
                Process = request.Process,
                DiscountPercentage = request.DiscountPercentage,
                PackingAmount = request.PackingAmount,
                PackingDescription = request.PackingDescription,
                TaxCode = request.TaxCode,
                TcsPercentage = request.TcsPercentage,
                FreightCharges = request.FreightCharges,
                VehicleNumber = request.VehicleNumber,
                TransportName = request.TransportName,
                IssueDate = request.IssueDate,
                RemovalDate = request.RemovalDate,
                IssueTime = request.IssueTime,
                RemovalTime = request.RemovalTime,
                StorageLocation = request.StorageLocation,
                Remarks = request.Remarks,
                LrNumber = request.LrNumber,
                LrDate = request.LrDate,
                CreditDays = request.CreditDays,
                AsnNumber = request.AsnNumber,
                NatureOfProduct = request.NatureOfProduct,
                PreparedBy = request.PreparedBy,
                ReworkFlag = request.ReworkFlag,
                TariffNumber = request.TariffNumber,
                TariffName = request.TariffName,
                TransportAmount = request.TransportAmount,
                CourierAmount = request.CourierAmount,
                DeliveryAddress = request.DeliveryAddress,
                AlternateCustomerCode = request.AlternateCustomerCode,
                OtherAmount = request.OtherAmount,
                BuyerName = request.BuyerName,
                BuyerAddress = request.BuyerAddress,
                InsuranceAmount = request.InsuranceAmount,
                AdvanceDuty = request.AdvanceDuty,
                OctriAmount = request.OctriAmount,
                ExportFlag = request.ExportFlag,
                FinalDestination = request.FinalDestination,
                PreCarriage = request.PreCarriage,
                PortOfLoading = request.PortOfLoading,
                PortOfDischarge = request.PortOfDischarge,
                PlaceOfDelivery = request.PlaceOfDelivery,
                CurrencyCode = request.CurrencyCode,
                CurrencyRate = request.CurrencyRate,
                Clearance = request.Clearance,
                FlightNumber = request.FlightNumber,
                ManufacturingDate = request.ManufacturingDate,
                ExpiryDate = request.ExpiryDate,
                AuthorizedSignatory = request.AuthorizedSignatory,
                AreaFormNumber = request.AreaFormNumber,
                FormDate = request.FormDate,
                Shipment = request.Shipment,
                CenvatAccountNumber = request.CenvatAccountNumber,
                BondNumber = request.BondNumber,
                BondDate = request.BondDate,
                Ut1FileNumber = request.Ut1FileNumber,
                FileNumber = request.FileNumber,
                ValidityDate = request.ValidityDate,
                ExaminationBoxes = request.ExaminationBoxes,
                TermsOfDelivery = request.TermsOfDelivery,
                TermsOfPayment = request.TermsOfPayment,
                VoyageNumber = request.VoyageNumber,
                PlaceOfReceipt = request.PlaceOfReceipt,
                MarksAndNumbers = request.MarksAndNumbers,
                NumberOfPackages = request.NumberOfPackages,
                UnNumber = request.UnNumber,
                HazardClass = request.HazardClass,
                HsCodeNumber = request.HsCodeNumber,
                ContainerNumber = request.ContainerNumber,
                SealNumber = request.SealNumber,
                OtsNumber = request.OtsNumber,
                CountryOfOrigin = request.CountryOfOrigin,
                CountryOfDestination = request.CountryOfDestination,
                TransportBy = request.TransportBy,
                AreaRemarks = request.AreaRemarks,
                CarrierName = request.CarrierName,
                CarrierBookingNumber = request.CarrierBookingNumber,
                ShipName = request.ShipName,
                TechnicalName = request.TechnicalName,
                OuterPackaging = request.OuterPackaging,
                InnerPackaging = request.InnerPackaging,
                SubsidiaryClass = request.SubsidiaryClass,
                UnPackingGroup = request.UnPackingGroup,
                UnPackingCode = request.UnPackingCode,
                EmsNumber = request.EmsNumber,
                FlashPoint = request.FlashPoint,
                MarinePollutant = request.MarinePollutant,
                ShipperDeclaration = request.ShipperDeclaration,
                IecNumber = request.IecNumber,
                IecDate = request.IecDate,
                CentralExciseRegistration = request.CentralExciseRegistration,
                DateOfExamination = request.DateOfExamination,
                SuperintendentExciseName = request.SuperintendentExciseName,
                InspectorExciseName = request.InspectorExciseName,
                CustomsSealNumber = request.CustomsSealNumber,
                PermitENumber = request.PermitENumber,
                NonCargoNumberOfPackages = request.NonCargoNumberOfPackages,
                ShippingBillNumber = request.ShippingBillNumber,
                LcNumber = request.LcNumber,
                LcDate = request.LcDate,
                TransportOwner = request.TransportOwner,
                TransportAddress = request.TransportAddress,
                TNumber = request.TNumber,
                TrayCode = request.TrayCode,
                TrayQuantity = request.TrayQuantity,
                Address = request.Address,
                StateCode = request.StateCode,
                AddressSelected = request.AddressSelected,
                HsnCode = request.HsnCode,
                ElectronicReferenceNumber = request.ElectronicReferenceNumber,
                TermsAndConditions = request.TermsAndConditions,
                AuthorizedName = request.AuthorizedName,
                ServicePercentage = request.ServicePercentage,
                ServiceEducationCessPercentage = request.ServiceEducationCessPercentage,
                ServiceHigherEducationCessPercentage = request.ServiceHigherEducationCessPercentage,
                InvoiceDetails = request.InvoiceDetails.Select(d => new CreateTaxInvoiceDetailCommand
                {
                    ItemCode = d.ItemCode,
                    UomCode = d.UomCode,
                    InvoiceQuantity = d.InvoiceQuantity,
                    Rate = d.Rate,
                    CustomerPoCode = d.CustomerPoCode,
                    ConversionQuantity = d.ConversionQuantity,
                    AmortizationRate = d.AmortizationRate,
                    NumberOfPackages = d.NumberOfPackages,
                    PackageDescription = d.PackageDescription,
                    QuantityPerPack = d.QuantityPerPack,
                    DeliveryChallanNumbers = d.DeliveryChallanNumbers,
                    DeliveryChallanDates = d.DeliveryChallanDates,
                    ExciseNumbers = d.ExciseNumbers,
                    ProcessCode = d.ProcessCode,
                    GinNumber = d.GinNumber,
                    GinDate = d.GinDate,
                    GinReceipt = d.GinReceipt,
                    MrCode = d.MrCode,
                    GinAcceptance = d.GinAcceptance,
                    CgstPercentage = d.CgstPercentage,
                    SgstPercentage = d.SgstPercentage,
                    IgstPercentage = d.IgstPercentage,
                    SerialNumber = d.SerialNumber,
                    Remarks = d.Remarks,
                    ItemWarehouseCode = d.ItemWarehouseCode,
                    ActualWeight = d.ActualWeight,
                    Size = d.Size,
                    SubHeading = d.SubHeading,
                    BatchNumber = d.BatchNumber,
                    PackingQuantity = d.PackingQuantity,
                    GrossWeight = d.GrossWeight,
                    NetWeight = d.NetWeight,
                    SizeOfBox = d.SizeOfBox,
                    NumberOfBarrels = d.NumberOfBarrels,
                    NumberOfPackagesDescription = d.NumberOfPackagesDescription,
                    ContainerNumber = d.ContainerNumber,
                    RefundableQuantity = d.RefundableQuantity,
                    AmortRate = d.AmortRate,
                    HsnCode = d.HsnCode,
                    StoreCode = d.StoreCode
                }).ToList()
            };
        }

        private UpdateTaxInvoiceCommand MapUpdateRequestToCommand(UpdateTaxInvoiceRequest request)
        {
            return new UpdateTaxInvoiceCommand
            {
                InvoiceCode = request.InvoiceCode,
                CompanyCode = request.CompanyCode,
                InvoiceDate = request.InvoiceDate,
                CustomerCode = request.CustomerCode,
                CustomerPoCode = request.CustomerPoCode,
                InvoiceType = request.InvoiceType,
                Type = request.Type,
                PaymentMethodCode = request.PaymentMethodCode,
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
                IsSupplementary = request.IsSupplementary,
                Process = request.Process,
                DiscountPercentage = request.DiscountPercentage,
                PackingAmount = request.PackingAmount,
                PackingDescription = request.PackingDescription,
                TaxCode = request.TaxCode,
                TcsPercentage = request.TcsPercentage,
                FreightCharges = request.FreightCharges,
                VehicleNumber = request.VehicleNumber,
                TransportName = request.TransportName,
                IssueDate = request.IssueDate,
                RemovalDate = request.RemovalDate,
                IssueTime = request.IssueTime,
                RemovalTime = request.RemovalTime,
                StorageLocation = request.StorageLocation,
                Remarks = request.Remarks,
                LrNumber = request.LrNumber,
                LrDate = request.LrDate,
                CreditDays = request.CreditDays,
                AsnNumber = request.AsnNumber,
                NatureOfProduct = request.NatureOfProduct,
                PreparedBy = request.PreparedBy,
                ReworkFlag = request.ReworkFlag,
                TariffNumber = request.TariffNumber,
                TariffName = request.TariffName,
                TransportAmount = request.TransportAmount,
                CourierAmount = request.CourierAmount,
                DeliveryAddress = request.DeliveryAddress,
                AlternateCustomerCode = request.AlternateCustomerCode,
                OtherAmount = request.OtherAmount,
                BuyerName = request.BuyerName,
                BuyerAddress = request.BuyerAddress,
                InsuranceAmount = request.InsuranceAmount,
                AdvanceDuty = request.AdvanceDuty,
                OctriAmount = request.OctriAmount,
                ExportFlag = request.ExportFlag,
                FinalDestination = request.FinalDestination,
                PreCarriage = request.PreCarriage,
                PortOfLoading = request.PortOfLoading,
                PortOfDischarge = request.PortOfDischarge,
                PlaceOfDelivery = request.PlaceOfDelivery,
                CurrencyCode = request.CurrencyCode,
                CurrencyRate = request.CurrencyRate,
                Clearance = request.Clearance,
                FlightNumber = request.FlightNumber,
                ManufacturingDate = request.ManufacturingDate,
                ExpiryDate = request.ExpiryDate,
                AuthorizedSignatory = request.AuthorizedSignatory,
                AreaFormNumber = request.AreaFormNumber,
                FormDate = request.FormDate,
                Shipment = request.Shipment,
                CenvatAccountNumber = request.CenvatAccountNumber,
                BondNumber = request.BondNumber,
                BondDate = request.BondDate,
                Ut1FileNumber = request.Ut1FileNumber,
                FileNumber = request.FileNumber,
                ValidityDate = request.ValidityDate,
                ExaminationBoxes = request.ExaminationBoxes,
                TermsOfDelivery = request.TermsOfDelivery,
                TermsOfPayment = request.TermsOfPayment,
                VoyageNumber = request.VoyageNumber,
                PlaceOfReceipt = request.PlaceOfReceipt,
                MarksAndNumbers = request.MarksAndNumbers,
                NumberOfPackages = request.NumberOfPackages,
                UnNumber = request.UnNumber,
                HazardClass = request.HazardClass,
                HsCodeNumber = request.HsCodeNumber,
                ContainerNumber = request.ContainerNumber,
                SealNumber = request.SealNumber,
                OtsNumber = request.OtsNumber,
                CountryOfOrigin = request.CountryOfOrigin,
                CountryOfDestination = request.CountryOfDestination,
                TransportBy = request.TransportBy,
                AreaRemarks = request.AreaRemarks,
                CarrierName = request.CarrierName,
                CarrierBookingNumber = request.CarrierBookingNumber,
                ShipName = request.ShipName,
                TechnicalName = request.TechnicalName,
                OuterPackaging = request.OuterPackaging,
                InnerPackaging = request.InnerPackaging,
                SubsidiaryClass = request.SubsidiaryClass,
                UnPackingGroup = request.UnPackingGroup,
                UnPackingCode = request.UnPackingCode,
                EmsNumber = request.EmsNumber,
                FlashPoint = request.FlashPoint,
                MarinePollutant = request.MarinePollutant,
                ShipperDeclaration = request.ShipperDeclaration,
                IecNumber = request.IecNumber,
                IecDate = request.IecDate,
                CentralExciseRegistration = request.CentralExciseRegistration,
                DateOfExamination = request.DateOfExamination,
                SuperintendentExciseName = request.SuperintendentExciseName,
                InspectorExciseName = request.InspectorExciseName,
                CustomsSealNumber = request.CustomsSealNumber,
                PermitENumber = request.PermitENumber,
                NonCargoNumberOfPackages = request.NonCargoNumberOfPackages,
                ShippingBillNumber = request.ShippingBillNumber,
                LcNumber = request.LcNumber,
                LcDate = request.LcDate,
                TransportOwner = request.TransportOwner,
                TransportAddress = request.TransportAddress,
                TNumber = request.TNumber,
                TrayCode = request.TrayCode,
                TrayQuantity = request.TrayQuantity,
                Address = request.Address,
                StateCode = request.StateCode,
                AddressSelected = request.AddressSelected,
                HsnCode = request.HsnCode,
                ElectronicReferenceNumber = request.ElectronicReferenceNumber,
                TermsAndConditions = request.TermsAndConditions,
                AuthorizedName = request.AuthorizedName,
                ServicePercentage = request.ServicePercentage,
                ServiceEducationCessPercentage = request.ServiceEducationCessPercentage,
                ServiceHigherEducationCessPercentage = request.ServiceHigherEducationCessPercentage,
                InvoiceDetails = request.InvoiceDetails.Select(d => new CreateTaxInvoiceDetailCommand
                {
                    ItemCode = d.ItemCode,
                    UomCode = d.UomCode,
                    InvoiceQuantity = d.InvoiceQuantity,
                    Rate = d.Rate,
                    CustomerPoCode = d.CustomerPoCode,
                    ConversionQuantity = d.ConversionQuantity,
                    AmortizationRate = d.AmortizationRate,
                    NumberOfPackages = d.NumberOfPackages,
                    PackageDescription = d.PackageDescription,
                    QuantityPerPack = d.QuantityPerPack,
                    DeliveryChallanNumbers = d.DeliveryChallanNumbers,
                    DeliveryChallanDates = d.DeliveryChallanDates,
                    ExciseNumbers = d.ExciseNumbers,
                    ProcessCode = d.ProcessCode,
                    GinNumber = d.GinNumber,
                    GinDate = d.GinDate,
                    GinReceipt = d.GinReceipt,
                    MrCode = d.MrCode,
                    GinAcceptance = d.GinAcceptance,
                    CgstPercentage = d.CgstPercentage,
                    SgstPercentage = d.SgstPercentage,
                    IgstPercentage = d.IgstPercentage,
                    SerialNumber = d.SerialNumber,
                    Remarks = d.Remarks,
                    ItemWarehouseCode = d.ItemWarehouseCode,
                    ActualWeight = d.ActualWeight,
                    Size = d.Size,
                    SubHeading = d.SubHeading,
                    BatchNumber = d.BatchNumber,
                    PackingQuantity = d.PackingQuantity,
                    GrossWeight = d.GrossWeight,
                    NetWeight = d.NetWeight,
                    SizeOfBox = d.SizeOfBox,
                    NumberOfBarrels = d.NumberOfBarrels,
                    NumberOfPackagesDescription = d.NumberOfPackagesDescription,
                    ContainerNumber = d.ContainerNumber,
                    RefundableQuantity = d.RefundableQuantity,
                    AmortRate = d.AmortRate,
                    HsnCode = d.HsnCode,
                    StoreCode = d.StoreCode
                }).ToList()
            };
        }

        #endregion

        #region Print Tax Invoice

        /// <summary>
        /// Print Tax Invoice as PDF (Single)
        /// </summary>
        /// <param name="invoiceCode">Invoice Code</param>
        /// <param name="companyId">Company ID</param>
        /// <param name="copyType">Copy Type (1=Original, 2=Duplicate, 3=Triplicate, 4=ExtraCopy)</param>
        /// <returns>PDF file</returns>
        [HttpGet("{invoiceCode:long}/print")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.Print)]
        public async Task<IActionResult> PrintTaxInvoice(
            long invoiceCode,
            [FromQuery] int companyId,
            [FromQuery] InvoiceCopyType copyType = InvoiceCopyType.Original)
        {
            _logger.LogInformation("GET /api/TaxInvoice/{InvoiceCode}/print - Printing Invoice: {InvoiceCode}, Company: {CompanyId}, CopyType: {CopyType}",
                invoiceCode, invoiceCode, companyId, copyType);

            try
            {
                // Get print data
                var query = new GetTaxInvoicePrintDataQuery
                {
                    InvoiceCode = invoiceCode,
                    CompanyId = companyId,
                    CopyType = copyType
                };

                var printData = await _mediator.Send(query);

                if (printData == null)
                {
                    return NotFound(new { message = $"Tax Invoice with code '{invoiceCode}' not found." });
                }

                // Generate PDF
                var pdfBytes = _pdfService.GenerateTaxInvoicePdf(printData);

                // Return PDF file
                return File(pdfBytes, "application/pdf", $"TaxInvoice_{printData.InvoiceHeader.InvoiceSerialNo}_{copyType}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing Tax Invoice {InvoiceCode}", invoiceCode);
                return StatusCode(500, new { message = "An error occurred while generating the PDF.", error = ex.Message });
            }
        }

        /// <summary>
        /// Print Multiple Tax Invoices as a single PDF
        /// </summary>
        /// <param name="request">Print request with invoice codes and copy types</param>
        /// <returns>Merged PDF file</returns>
        [HttpPost("print-batch")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.Print)]
        public async Task<IActionResult> PrintBatchTaxInvoices([FromBody] BatchPrintRequest request)
        {
            _logger.LogInformation("POST /api/TaxInvoice/print-batch - Printing {Count} invoices", request.Invoices.Count);

            try
            {
                var printDataList = new List<TaxInvoicePrintDto>();

                foreach (var invoiceRequest in request.Invoices)
                {
                    var query = new GetTaxInvoicePrintDataQuery
                    {
                        InvoiceCode = invoiceRequest.InvoiceCode,
                        CompanyId = request.CompanyId,
                        CopyType = invoiceRequest.CopyType
                    };

                    var printData = await _mediator.Send(query);
                    
                    if (printData != null)
                    {
                        printDataList.Add(printData);
                    }
                }

                if (printDataList.Count == 0)
                {
                    return NotFound(new { message = "No valid invoices found for printing." });
                }

                // Generate merged PDF
                var pdfBytes = _pdfService.GenerateBatchTaxInvoicePdf(printDataList);

                // Return PDF file
                return File(pdfBytes, "application/pdf", $"TaxInvoices_Batch_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing batch Tax Invoices");
                return StatusCode(500, new { message = "An error occurred while generating the batch PDF.", error = ex.Message });
            }
        }

        #endregion

        #region Lock / Unlock

        [HttpPost("{id:long}/lock")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.Edit)]
        public async Task<IActionResult> LockTaxInvoice(long id, [FromQuery] int companyId)
        {
            _logger.LogInformation("POST /api/TaxInvoice/{Id}/lock - Locking invoice", id);
            var userCode = int.TryParse(User.FindFirst("user_code")?.Value, out var uc) ? uc : 0;
            var result = await _mediator.Send(new LockTaxInvoiceCommand { InvoiceCode = id, LockedByUserId = userCode });
            if (!result)
                return Conflict(new { message = $"Tax Invoice {id} is already locked by another user." });
            return Ok(new { message = "Invoice locked successfully." });
        }

        [HttpPost("{id:long}/unlock")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.Edit)]
        public async Task<IActionResult> UnlockTaxInvoice(long id, [FromQuery] int companyId)
        {
            _logger.LogInformation("POST /api/TaxInvoice/{Id}/unlock - Unlocking invoice", id);
            var result = await _mediator.Send(new UnlockTaxInvoiceCommand { InvoiceCode = id });
            if (!result)
                return NotFound(new { message = $"Tax Invoice {id} not found or could not be unlocked." });
            return Ok(new { message = "Invoice unlocked successfully." });
        }

        #endregion

        #region Approve

        /// <summary>
        /// Approve a Tax Invoice (requires BackDate permission — bit 6)
        /// </summary>
        [HttpPost("{id:long}/approve")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.BackDate)]
        public async Task<IActionResult> ApproveTaxInvoice(long id, [FromQuery] int companyCode)
        {
            var approved = await _mediator.Send(new ApproveTaxInvoiceCommand
            {
                InvoiceCode = id,
                CompanyCode = companyCode
            });

            if (!approved)
                return NotFound(new { message = $"Tax Invoice {id} not found or already deleted." });

            return Ok(new { message = $"Tax Invoice {id} approved successfully." });
        }

        #endregion
    }
}

/// <summary>
/// Request model for batch printing
/// </summary>
public class BatchPrintRequest
{
    public int CompanyId { get; set; }
    public List<InvoicePrintRequest> Invoices { get; set; } = new();
}

/// <summary>
/// Single invoice print request
/// </summary>
public class InvoicePrintRequest
{
    public long InvoiceCode { get; set; }
    public InvoiceCopyType CopyType { get; set; } = InvoiceCopyType.Original;
}

