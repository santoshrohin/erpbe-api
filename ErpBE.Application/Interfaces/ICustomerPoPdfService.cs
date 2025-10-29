using ErpBE.Application.DTOs;

namespace ErpBE.Application.Interfaces;

/// <summary>
/// Interface for Customer PO (Sales Order) PDF generation service
/// </summary>
public interface ICustomerPoPdfService
{
    /// <summary>
    /// Generates a PDF for a single Customer PO
    /// </summary>
    /// <param name="data">Customer PO print data</param>
    /// <returns>PDF as byte array</returns>
    byte[] GenerateCustomerPoPdf(CustomerPoPrintDto data);

    /// <summary>
    /// Generates a PDF for multiple Customer POs (batch printing)
    /// </summary>
    /// <param name="pos">List of Customer PO print data</param>
    /// <returns>Combined PDF as byte array</returns>
    byte[] GenerateBatchCustomerPoPdf(List<CustomerPoPrintDto> pos);
}

