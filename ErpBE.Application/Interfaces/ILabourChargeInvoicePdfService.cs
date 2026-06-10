using ErpBE.Application.DTOs;

namespace ErpBE.Application.Interfaces;

public interface ILabourChargeInvoicePdfService
{
    byte[] GenerateLabourChargeInvoicePdf(LabourChargeInvoicePrintDto data);
}
