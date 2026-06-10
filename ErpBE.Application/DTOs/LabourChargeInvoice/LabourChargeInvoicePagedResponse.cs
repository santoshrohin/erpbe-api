namespace ErpBE.Application.DTOs.LabourChargeInvoice
{
    public class LabourChargeInvoicePagedResponse
    {
        public List<LabourChargeInvoiceMasterDto> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
