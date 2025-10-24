namespace ErpBE.Application.DTOs
{
    /// <summary>
    /// Tax Invoice Detail DTO - COMPLETE representation of ALL fields in INVOICE_DETAIL
    /// Includes all line item fields from legacy application
    /// </summary>
    public class TaxInvoiceDetailDto
    {
        #region Primary Keys & References (4 fields)
        
        public int? InvoiceMasterCode { get; set; } // IND_INM_CODE
        public int? ItemCode { get; set; } // IND_I_CODE
        public int? UomCode { get; set; } // IND_UOM_CODE
        public int? CustomerPoCode { get; set; } // IND_CPOM_CODE
        
        #endregion
        
        #region Quantity & Rate (8 fields)
        
        public double InvoiceQuantity { get; set; } // IND_INQTY (NOT NULL - Required)
        public double? Rate { get; set; } // IND_RATE
        public double? ConversionQuantity { get; set; } // IND_CON_QTY
        public double? AmortizationRate { get; set; } // IND_AMORT_RATE
        public int? NumberOfPackages { get; set; } // IND_NO_PACK
        public string? PackageDescription { get; set; } // IND_PACK_DESC
        public decimal? QuantityPerPack { get; set; } // IND_QTY_PACK
        public double? Amount { get; set; } // IND_AMT
        
        #endregion
        
        #region Delivery Challan (3 fields)
        
        public string? DeliveryChallanNumbers { get; set; } // IND_DC_NO (comma-separated)
        public string? DeliveryChallanDates { get; set; } // IND_DC_DATE (comma-separated)
        public string? ExciseNumbers { get; set; } // IND_EX_NO (comma-separated)
        
        #endregion
        
        #region Process & GIN (6 fields)
        
        public int? ProcessCode { get; set; } // IND_PROCESS_CODE
        public string? GinNumber { get; set; } // IND_GIN_NO
        public DateTime? GinDate { get; set; } // IND_GIN_DATE
        public double? GinReceipt { get; set; } // IND_GIN_RCPT
        public decimal? MrCode { get; set; } // IND_MR_CODE
        public double? GinAcceptance { get; set; } // IND_GIN_ACCP
        
        #endregion
        
        #region Tax Amounts (6 fields)
        
        public double? ExciseAmount { get; set; } // IND_EX_AMT
        public double? EducationCessAmount { get; set; } // IND_E_CESS_AMT
        public double? SecondaryHigherEducationCessAmount { get; set; } // IND_SH_CESS_AMT
        public double? CgstPercentage { get; set; } // E_BASIC_CentralT (CGST %)
        public double? SgstPercentage { get; set; } // E_EDU_CESS_State (SGST %)
        public double? IgstPercentage { get; set; } // E_H_EDU_Integrated (IGST %)
        
        #endregion
        
        #region Additional Details (9 fields)
        
        public string? SerialNumber { get; set; } // IND_SR_NO
        public string? Remarks { get; set; } // IND_REMARK
        public int? ItemWarehouseCode { get; set; } // IND_IWM_CODE
        public bool? IsDeleted { get; set; } // ES_DELETE
        public double? ActualWeight { get; set; } // IND_ACT_WEIGHT
        public string? Size { get; set; } // IND_SIZE
        public string? SubHeading { get; set; } // IND_SUBHEADING
        public string? BatchNumber { get; set; } // IND_BACHNO
        public double? PackingQuantity { get; set; } // IND_PAK_QTY
        
        #endregion
        
        #region Weight & Packing (6 fields)
        
        public double? GrossWeight { get; set; } // IND_GROSS_WEIGHT
        public double? NetWeight { get; set; } // IND_NET_WEIGHT
        public double? SizeOfBox { get; set; } // IND_SIZE_OF_BOX
        public double? NumberOfBarrels { get; set; } // IND_NO_OF_BARRELS
        public string? NumberOfPackagesDescription { get; set; } // IND_NO_OF_PACK_DESC
        public string? ContainerNumber { get; set; } // IND_CONTAINER_NO
        
        #endregion
        
        #region Amortization & Refund (3 fields)
        
        public double? RefundableQuantity { get; set; } // IND_REFUNDABLE_QTY
        public double? AmortRate { get; set; } // IND_AMORTRATE
        public double? AmortAmount { get; set; } // IND_AMORTAMT
        
        #endregion
        
        #region HSN & Store (2 fields)
        
        public string? HsnCode { get; set; } // IND_HSN_CODE
        public int? StoreCode { get; set; } // IND_STORE_CODE
        
        #endregion
        
        #region Navigation/Display Properties (Not in DB)
        
        public string? ItemCode_Display { get; set; } // I_CODENO (for display)
        public string? ItemName { get; set; } // I_NAME (for display)
        public string? UomName { get; set; } // I_UOM_NAME (for display)
        public string? CustomerPoNumber { get; set; } // CPOM_PONO (for display)
        public double? StockQuantity { get; set; } // For display in grid
        public double? PendingQuantity { get; set; } // For display in grid
        
        #endregion
    }
}
