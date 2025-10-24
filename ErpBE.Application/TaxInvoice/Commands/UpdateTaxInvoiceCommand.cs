using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.TaxInvoice.Commands
{
    public class UpdateTaxInvoiceCommand : IRequest<TaxInvoiceMasterDto>
    {
        public int InvoiceCode { get; set; } // PK - Required
        
        #region Basic Details (Required)
        
        public int CompanyCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int CustomerCode { get; set; }
        public int? CustomerPoCode { get; set; }
        
        #endregion
        
        #region Optional Header Fields
        
        public byte? InvoiceType { get; set; }
        public string? Type { get; set; }
        public int? PaymentMethodCode { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool? IsSupplementary { get; set; }
        public string? Process { get; set; }
        
        #endregion
        
        #region Amount Fields
        
        public double? DiscountPercentage { get; set; }
        public double? PackingAmount { get; set; }
        public string? PackingDescription { get; set; }
        public int? TaxCode { get; set; }
        public double? TcsPercentage { get; set; }
        
        #endregion
        
        #region Transport & Logistics
        
        public double? FreightCharges { get; set; }
        public string? VehicleNumber { get; set; }
        public string? TransportName { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? RemovalDate { get; set; }
        public string? IssueTime { get; set; }
        public string? RemovalTime { get; set; }
        public string? StorageLocation { get; set; }
        public string? Remarks { get; set; }
        public string? LrNumber { get; set; }
        public DateTime? LrDate { get; set; }
        
        #endregion
        
        #region Additional Details
        
        public int? CreditDays { get; set; }
        public string? AsnNumber { get; set; }
        public string? NatureOfProduct { get; set; }
        public string? PreparedBy { get; set; }
        public bool? ReworkFlag { get; set; }
        public string? TariffNumber { get; set; }
        public string? TariffName { get; set; }
        
        #endregion
        
        #region Additional Charges
        
        public double? TransportAmount { get; set; }
        public double? CourierAmount { get; set; }
        public string? DeliveryAddress { get; set; }
        public int? AlternateCustomerCode { get; set; }
        public double? OtherAmount { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerAddress { get; set; }
        public double? InsuranceAmount { get; set; }
        public double? AdvanceDuty { get; set; }
        public double? OctriAmount { get; set; }
        
        #endregion
        
        #region Export Fields
        
        public bool? ExportFlag { get; set; }
        public string? FinalDestination { get; set; }
        public string? PreCarriage { get; set; }
        public string? PortOfLoading { get; set; }
        public string? PortOfDischarge { get; set; }
        public string? PlaceOfDelivery { get; set; }
        public int? CurrencyCode { get; set; }
        public double? CurrencyRate { get; set; }
        public string? Clearance { get; set; }
        public string? FlightNumber { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? AuthorizedSignatory { get; set; }
        public string? AreaFormNumber { get; set; }
        public DateTime? FormDate { get; set; }
        public string? Shipment { get; set; }
        public string? CenvatAccountNumber { get; set; }
        public string? BondNumber { get; set; }
        public DateTime? BondDate { get; set; }
        public string? Ut1FileNumber { get; set; }
        public string? FileNumber { get; set; }
        public DateTime? ValidityDate { get; set; }
        public string? ExaminationBoxes { get; set; }
        public string? TermsOfDelivery { get; set; }
        public string? TermsOfPayment { get; set; }
        public string? VoyageNumber { get; set; }
        public string? PlaceOfReceipt { get; set; }
        public string? MarksAndNumbers { get; set; }
        public string? NumberOfPackages { get; set; }
        public string? UnNumber { get; set; }
        public string? HazardClass { get; set; }
        public string? HsCodeNumber { get; set; }
        public string? ContainerNumber { get; set; }
        public string? SealNumber { get; set; }
        public string? OtsNumber { get; set; }
        public int? CountryOfOrigin { get; set; }
        public int? CountryOfDestination { get; set; }
        public string? TransportBy { get; set; }
        public string? AreaRemarks { get; set; }
        public string? CarrierName { get; set; }
        public string? CarrierBookingNumber { get; set; }
        public string? ShipName { get; set; }
        public string? TechnicalName { get; set; }
        public string? OuterPackaging { get; set; }
        public string? InnerPackaging { get; set; }
        public string? SubsidiaryClass { get; set; }
        public string? UnPackingGroup { get; set; }
        public string? UnPackingCode { get; set; }
        public string? EmsNumber { get; set; }
        public string? FlashPoint { get; set; }
        public bool? MarinePollutant { get; set; }
        public string? ShipperDeclaration { get; set; }
        public string? IecNumber { get; set; }
        public DateTime? IecDate { get; set; }
        public string? CentralExciseRegistration { get; set; }
        public DateTime? DateOfExamination { get; set; }
        public string? SuperintendentExciseName { get; set; }
        public string? InspectorExciseName { get; set; }
        public string? CustomsSealNumber { get; set; }
        public string? PermitENumber { get; set; }
        public string? NonCargoNumberOfPackages { get; set; }
        public string? ShippingBillNumber { get; set; }
        public string? LcNumber { get; set; }
        public DateTime? LcDate { get; set; }
        
        #endregion
        
        #region Transport Details
        
        public string? TransportOwner { get; set; }
        public string? TransportAddress { get; set; }
        public string? TNumber { get; set; }
        
        #endregion
        
        #region Tray
        
        public int? TrayCode { get; set; }
        public double? TrayQuantity { get; set; }
        
        #endregion
        
        #region Address & HSN
        
        public string? Address { get; set; }
        public int? StateCode { get; set; }
        public int? AddressSelected { get; set; }
        public string? HsnCode { get; set; }
        public string? ElectronicReferenceNumber { get; set; }
        
        #endregion
        
        #region Terms & Conditions
        
        public string? TermsAndConditions { get; set; }
        public string? AuthorizedName { get; set; }
        
        #endregion
        
        #region Service Tax (Legacy)
        
        public double? ServicePercentage { get; set; }
        public double? ServiceEducationCessPercentage { get; set; }
        public double? ServiceHigherEducationCessPercentage { get; set; }
        
        #endregion
        
        #region Line Items (REQUIRED)
        
        public List<CreateTaxInvoiceDetailCommand> InvoiceDetails { get; set; } = new List<CreateTaxInvoiceDetailCommand>();
        
        #endregion
    }
}

