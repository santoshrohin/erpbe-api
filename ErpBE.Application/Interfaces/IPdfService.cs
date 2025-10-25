using ErpBE.Application.DTOs;

namespace ErpBE.Application.Interfaces;

/// <summary>
/// Service for generating PDF documents
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Generates a Tax Invoice PDF document
    /// </summary>
    /// <param name="invoiceData">Invoice data with all details</param>
    /// <returns>PDF as byte array</returns>
    byte[] GenerateTaxInvoicePdf(TaxInvoicePrintDto invoiceData);
    
    /// <summary>
    /// Generates multiple Tax Invoice PDFs merged into a single document
    /// </summary>
    /// <param name="invoices">List of invoice data</param>
    /// <returns>Merged PDF as byte array</returns>
    byte[] GenerateBatchTaxInvoicePdf(List<TaxInvoicePrintDto> invoices);
    
    /// <summary>
    /// Generates a QR Code image from text
    /// </summary>
    /// <param name="text">Text to encode in QR code</param>
    /// <param name="pixelsPerModule">Size of QR code</param>
    /// <returns>QR code as byte array (PNG image)</returns>
    byte[] GenerateQRCode(string text, int pixelsPerModule = 20);
}

