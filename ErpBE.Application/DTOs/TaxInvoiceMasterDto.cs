namespace ErpBE.Application.DTOs
{
    /// <summary>
    /// Tax Invoice Master DTO - COMPLETE representation of ALL 157 fields in INVOICE_MASTER
    /// Includes domestic, export, e-invoice, and all other fields from legacy application
    /// </summary>
    public class TaxInvoiceMasterDto
    {
        #region Primary & Basic Details (10 fields)
        
        public int InvoiceCode { get; set; } // INM_CODE (PK)
        public int? CompanyCode { get; set; } // INM_CM_CODE
        public int? InvoiceNumber { get; set; } // INM_NO
        public DateTime? InvoiceDate { get; set; } // INM_DATE
        public byte? InvoiceType { get; set; } // INM_INVOICE_TYPE (0 for Tax Invoice)
        public string? Type { get; set; } // INM_TYPE ('TAXINV')
        public int? PaymentMethodCode { get; set; } // INM_PMM_CODE
        public int? CustomerCode { get; set; } // INM_P_CODE
        public DateTime? DateFrom { get; set; } // INM_DF_DATE
        public DateTime? DateTo { get; set; } // INM_DT_DATE
        
        #endregion
        
        #region PO & Process (3 fields)
        
        public int? CustomerPoCode { get; set; } // INM_CPOM_CODE
        public bool? IsSupplementary { get; set; } // INM_SUPPLEMENTORY
        public string? Process { get; set; } // INM_PROCESS
        
        #endregion
        
        #region Amount Fields (18 fields)
        
        public double? NetAmount { get; set; } // INM_NET_AMT
        public double? ServiceTaxPercentage { get; set; } // INM_S_TAX
        public double? ServiceTaxAmount { get; set; } // INM_S_TAX_AMT
        public double? BasicExcisePercentage { get; set; } // INM_BEXCISE (CGST %)
        public double? BasicExciseAmount { get; set; } // INM_BE_AMT (CGST Amount)
        public double? EducationCessPercentage { get; set; } // INM_EDUC_CESS (SGST %)
        public double? EducationCessAmount { get; set; } // INM_EDUC_AMT (SGST Amount)
        public double? HigherEducationCessPercentage { get; set; } // INM_H_EDUC_CESS (IGST %)
        public double? HigherEducationCessAmount { get; set; } // INM_H_EDUC_AMT (IGST Amount)
        public double? DiscountPercentage { get; set; } // INM_DISC
        public double? DiscountAmount { get; set; } // INM_DISC_AMT
        public double? PackingAmount { get; set; } // INM_PACK_AMT
        public string? PackingDescription { get; set; } // INM_PACK_DCES
        public int? TaxCode { get; set; } // INM_T_CODE
        public double? TaxAmount { get; set; } // INM_T_AMT
        public double? GrossAmount { get; set; } // INM_G_AMT
        public double? GrossAmortizationAmount { get; set; } // INM_G_AMORT_AMT
        public double? TcsPercentage { get; set; } // INM_TAX_TCS
        public double? TcsAmount { get; set; } // INM_TAX_TCS_AMT
        
        #endregion
        
        #region Transport & Logistics (7 fields)
        
        public double? FreightCharges { get; set; } // INM_FREIGHT
        public string? VehicleNumber { get; set; } // INM_VEH_NO
        public string? TransportName { get; set; } // INM_TRANSPORT
        public DateTime? IssueDate { get; set; } // INM_ISSUE_DATE
        public DateTime? RemovalDate { get; set; } // INM_REMOVAL_DATE
        public string? StorageLocation { get; set; } // INM_STO_LOC (Amortization Amount)
        public string? Remarks { get; set; } // INM_REMARK
        
        #endregion
        
        #region Additional Details (10 fields)
        
        public int? CreditDays { get; set; } // INM_C_DAYS
        public string? AsnNumber { get; set; } // INM_ASN_NO
        public string? NatureOfProduct { get; set; } // INM_NATURE_PRO
        public string? PreparedBy { get; set; } // INM_PREPARE_BY
        public bool? ReworkFlag { get; set; } // INM_REWORK_FLAG
        public byte? TallyTransferFlag { get; set; } // INM_TALLYTNF
        public byte? TempTallyTransferFlag { get; set; } // INM_TEMP_TALLYTNF
        public string? TariffNumber { get; set; } // INM_TRIFF_NO
        public string? TariffName { get; set; } // INM_TRIFF_NAME
        public byte? TallyTransferFlag1 { get; set; } // INM_TALLYTNF1
        public byte? TempTallyTransferFlag1 { get; set; } // INM_TEMP_TALLYTNF1
        
        #endregion
        
        #region System Fields (2 fields)
        
        public bool? IsDeleted { get; set; } // ES_DELETE
        public bool? IsModifyLocked { get; set; } // MODIFY
        
        #endregion
        
        #region Additional Charges (9 fields)
        
        public double? TransportAmount { get; set; } // INM_TRANS_AMT
        public double? CourierAmount { get; set; } // INM_COURIER_AMT
        public double? ServicePercentage { get; set; } // INM_SER_PER
        public double? ServiceAmount { get; set; } // INM_SER_AMT
        public double? ServiceEducationCessPercentage { get; set; } // INM_SER_EDUC_CESS
        public double? ServiceEducationCessAmount { get; set; } // INM_SER_EDUC_CESS_AMT
        public double? ServiceHigherEducationCessPercentage { get; set; } // INM_SER_H_EDUC_CESS
        public double? ServiceHigherEducationCessAmount { get; set; } // INM_SER_H_EDUC_CESS_AMT
        public string? DeliveryAddress { get; set; } // INM_DEL_ADD
        
        #endregion
        
        #region LR & Other (5 fields)
        
        public int? AlternateCustomerCode { get; set; } // INM_P_CODE_ALT
        public double? OtherAmount { get; set; } // INM_OTHER_AMT
        public string? LrNumber { get; set; } // INM_LR_NO
        public DateTime? LrDate { get; set; } // INM_LR_DATE
        public string? BuyerName { get; set; } // INM_BUYER_NAME
        
        #endregion
        
        #region Export Fields - Part 1 (20 fields)
        
        public string? BuyerAddress { get; set; } // INM_BUYTER_ADD
        public double? InsuranceAmount { get; set; } // INM_INSURANCE
        public string? FinalDestination { get; set; } // INM_FINAL_DEST
        public string? PreCarriage { get; set; } // INM_PRE_CARRIAGE
        public string? PortOfLoading { get; set; } // INM_PORT_OF_LOAD
        public string? PortOfDischarge { get; set; } // INM_PORT_OF_DISCH
        public string? PlaceOfDelivery { get; set; } // INM_PLACE_OF_DEL
        public int? CurrencyCode { get; set; } // INM_CURR_CODE
        public string? Clearance { get; set; } // INM_CLEARANCE
        public string? FlightNumber { get; set; } // INM_FLIGHT_NO
        public DateTime? ManufacturingDate { get; set; } // INM_MFG_DATE
        public DateTime? ExpiryDate { get; set; } // INM_EXP_DATE
        public double? CurrencyRate { get; set; } // INM_CURR_RATE
        public string? AuthorizedSignatory { get; set; } // INM_AUTHO_SIGN
        public string? AreaFormNumber { get; set; } // INM_AREA_FORM_NO
        public DateTime? FormDate { get; set; } // INM_FORM_DATE
        public string? Shipment { get; set; } // INM_SHIPMENT
        public string? CenvatAccountNumber { get; set; } // INM_CENVAT_AC_NO
        public string? BondNumber { get; set; } // INM_BOND_NO
        public DateTime? BondDate { get; set; } // INM_BOND_DATE
        
        #endregion
        
        #region Export Fields - Part 2 (20 fields)
        
        public string? Ut1FileNumber { get; set; } // INM_UT1_FILE_NO
        public string? FileNumber { get; set; } // INM_FILE_NO
        public bool? ExportFlag { get; set; } // INM_EXPORT_FLAG
        public DateTime? ValidityDate { get; set; } // INM_VALID_DATE
        public string? ExaminationBoxes { get; set; } // INM_EXA_BOXES
        public string? TermsOfDelivery { get; set; } // INM_TOD
        public string? TermsOfPayment { get; set; } // INM_TOP
        public string? VoyageNumber { get; set; } // INM_VOY_NO
        public string? PlaceOfReceipt { get; set; } // INM_PLACE_REC
        public string? MarksAndNumbers { get; set; } // INM_M_NO
        public string? NumberOfPackages { get; set; } // INM_NOS_PACK
        public string? UnNumber { get; set; } // INM_UN_NO
        public string? HazardClass { get; set; } // INM_HAZ
        public string? HsCodeNumber { get; set; } // INM_HS_CODENO
        public string? ContainerNumber { get; set; } // INM_CONTA_NO
        public string? SealNumber { get; set; } // INM_SEAL_NO
        public string? OtsNumber { get; set; } // INM_OTS_NO
        public int? CountryOfOrigin { get; set; } // INM_CONTRY_ORIGIN
        public int? CountryOfDestination { get; set; } // INM_COUNTRY_DEST
        public string? TransportBy { get; set; } // INM_TRANSPORT_BY
        
        #endregion
        
        #region Export Fields - Part 3 (20 fields)
        
        public string? AreaRemarks { get; set; } // INM_ARE_REMARK
        public string? CarrierName { get; set; } // INM_CARRIER_NAME
        public string? CarrierBookingNumber { get; set; } // INM_CARRIER_BOOK_NO
        public string? ShipName { get; set; } // INM_SHIP_NAME
        public string? TechnicalName { get; set; } // INM_TECH_NAME
        public string? OuterPackaging { get; set; } // INM_OUT_PAKGS
        public string? InnerPackaging { get; set; } // INM_INR_PAKG
        public string? SubsidiaryClass { get; set; } // INM_SUB_CLASS
        public string? UnPackingGroup { get; set; } // INM_UN_PAK_GRP
        public string? UnPackingCode { get; set; } // INM_UN_PAK_CODE
        public string? EmsNumber { get; set; } // INM_EMS_NO
        public string? FlashPoint { get; set; } // INM_FLASH_POINT
        public bool? MarinePollutant { get; set; } // INM_MARINE_POLLT
        public string? ShipperDeclaration { get; set; } // INM_SHIP_DECLAR
        public string? IecNumber { get; set; } // INM_IEC_NO
        public DateTime? IecDate { get; set; } // INM_IEC_DATE
        public string? CentralExciseRegistration { get; set; } // INM_CEN_EXC_REG
        public DateTime? DateOfExamination { get; set; } // INM_DATE_OF_EXAM
        public string? SuperintendentExciseName { get; set; } // INM_SUP_C_EXC_NAME
        public string? InspectorExciseName { get; set; } // INM_INSP_C_EXC_NAME
        
        #endregion
        
        #region Export Fields - Part 4 (10 fields)
        
        public string? CustomsSealNumber { get; set; } // INM_CUST_SEAL_NO
        public string? PermitENumber { get; set; } // INM_PER_E_NO
        public bool? IsTallyTransferred { get; set; } // INM_IS_TALLY_TRANS
        public string? NonCargoNumberOfPackages { get; set; } // INM_NONCARGO_NOPAKG
        public string? ShippingBillNumber { get; set; } // INM_SHIPP_BILL_NO
        public double? AccessibleAmount { get; set; } // INM_ACCESSIBLE_AMT
        public double? TaxableAmount { get; set; } // INM_TAXABLE_AMT
        public double? RoundingAmount { get; set; } // INM_ROUNDING_AMT
        public double? AdvanceDuty { get; set; } // INM_ADV_DUTY
        public double? OctriAmount { get; set; } // INM_OCTRI_AMT
        
        #endregion
        
        #region Supplementary & Time (6 fields)
        
        public bool? IsSuppliment { get; set; } // INM_IS_SUPPLIMENT (duplicate field)
        public string? IssueTime { get; set; } // INM_ISSU_TIME
        public string? RemovalTime { get; set; } // INM_REMOVEL_TIME
        public string? LcNumber { get; set; } // INM_LC_NO
        public DateTime? LcDate { get; set; } // INM_LC_DATE
        public string? TransportOwner { get; set; } // INM_TRANSPORT_OWNER
        
        #endregion
        
        #region Transport & Tray (8 fields)
        
        public string? TransportAddress { get; set; } // INM_TRANSPORT_ADDRESS
        public string? TNumber { get; set; } // INM_TNO
        public int? TrayCode { get; set; } // INM_TRAY_CODE
        public double? TrayQuantity { get; set; } // INM_TRAY_QTY
        public string? Address { get; set; } // INM_ADDRESS
        public int? StateCode { get; set; } // INM_STATE
        public string? HsnCode { get; set; } // INM_HSN_CODE
        public string? ElectronicReferenceNumber { get; set; } // INM_ELECTRREFNUM
        
        #endregion
        
        #region Terms & Conditions (3 fields)
        
        public string? TermsAndConditions { get; set; } // INM_TERMSNCONDITIONS
        public string? AuthorizedName { get; set; } // INM_AUTHORIZEDNAME
        public int? AddressSelected { get; set; } // INM_ADDRESS_SELECTED
        
        #endregion
        
        #region E-Invoice Fields (8 fields) - Structure ready, functionality later
        
        public string? AcknowledgementNumber { get; set; } // AckNo
        public string? AcknowledgementDate { get; set; } // AckDate
        public string? InvoiceValue { get; set; } // InvValue
        public string? RecipientGstin { get; set; } // ReciptGSTIn
        public string? EInvoiceStatus { get; set; } // EInvStatus
        public string? Irn { get; set; } // IRN
        public string? QrCode { get; set; } // QRCode
        public string? EwayBillNumber { get; set; } // EwayBill
        
        #endregion
        
        #region Navigation Properties
        
        // Customer Name (from PARTY_MASTER - for display only)
        public string? CustomerName { get; set; }
        
        // State Name (from STATE_MASTER - for display only)
        public string? StateName { get; set; }
        
        // Line Items
        public List<TaxInvoiceDetailDto> InvoiceDetails { get; set; } = new List<TaxInvoiceDetailDto>();
        
        #endregion
    }
}
