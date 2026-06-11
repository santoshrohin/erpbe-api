using ErpBE.Application.DTOs.LabourChargeInvoice;
using MediatR;

namespace ErpBE.Application.LabourChargeInvoice.Commands
{
    public class CreateLabourChargeInvoiceCommand : IRequest<LabourChargeInvoiceMasterDto>
    {
        public int CompanyCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int CustomerCode { get; set; }
        public int? CustomerPoCode { get; set; }
        public byte? InvoiceType { get; set; } = 0;
        public bool? IsSupplementary { get; set; }
        public double? NetAmount { get; set; }
        public double? DiscountPercentage { get; set; }
        public double? ServiceTaxPercentage { get; set; }
        public double? TcsPercentage { get; set; }
        public double? PackingAmount { get; set; }
        public double? GrossAmount { get; set; }
        public int? TaxCode { get; set; }
        public string? VehicleNumber { get; set; }
        public string? TransportName { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? RemovalDate { get; set; }
        public string? Remarks { get; set; }
        public string? LrNumber { get; set; }
        public DateTime? LrDate { get; set; }
        public double? TaxableAmount { get; set; }
        public double? RoundingAmount { get; set; }
        public double? OtherAmount { get; set; }
        public double? FreightCharges { get; set; }
        public double? InsuranceAmount { get; set; }
        public double? TransportAmount { get; set; }
        public double? OctriAmount { get; set; }
        public int? CreditDays { get; set; }
        public string? HsnCode { get; set; }
        public double? CgstPercentage { get; set; }
        public double? SgstPercentage { get; set; }
        public double? IgstPercentage { get; set; }
        public double? AccessibleAmount { get; set; }
        public double? DiscountAmount { get; set; }
        public string? IssueTime { get; set; }
        public string? RemovalTime { get; set; }
        public List<CreateLabourChargeInvoiceDetailRequest> Details { get; set; } = new();
    }
}
