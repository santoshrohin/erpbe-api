namespace ErpBE.Application.DTOs.TaxInvoice
{
    /// <summary>
    /// Request model for updating an existing Tax Invoice (with ALL fields)
    /// </summary>
    public class UpdateTaxInvoiceRequest : CreateTaxInvoiceRequest
    {
        public int InvoiceCode { get; set; } // PK - Required for update
    }
}

