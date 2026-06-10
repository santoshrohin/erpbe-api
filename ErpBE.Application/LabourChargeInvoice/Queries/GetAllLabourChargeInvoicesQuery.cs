using ErpBE.Application.DTOs.LabourChargeInvoice;
using MediatR;

namespace ErpBE.Application.LabourChargeInvoice.Queries
{
    public class GetAllLabourChargeInvoicesQuery : IRequest<LabourChargeInvoicePagedResponse>
    {
        public LabourChargeInvoiceQueryParameters Parameters { get; set; } = new();
    }
}
