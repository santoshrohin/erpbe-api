using ErpBE.Application.DTOs;

namespace ErpBE.Application.Interfaces;

public interface IDeliveryChallanPdfService
{
    byte[] GenerateDeliveryChallanPdf(DeliveryChallanPrintDto data);
}
