using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;

namespace ErpBE.Infrastructure.Services;

/// <summary>
/// Service for generating Tax Invoice PDF documents using QuestPDF
/// MATCHES EXACT FORMAT from taxinvoice_page-0001.jpg
/// </summary>
public class TaxInvoicePdfService : IPdfService
{
    public TaxInvoicePdfService()
    {
        // Set QuestPDF license (Community License)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateTaxInvoicePdf(TaxInvoicePrintDto data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(10);
                
                page.Content().Column(column =>
                {
                    // Main invoice content with double border
                    column.Item().Border(2).BorderColor("#000000").Padding(2)
                        .Border(1).BorderColor("#000000").Padding(5)
                        .Column(innerColumn =>
                        {
                            // 1. Header Section
                            ComposeHeader(innerColumn, data);
                            
                            // 2. Invoice Header (Two columns)
                            ComposeInvoiceHeader(innerColumn, data);
                            
                            // 3. Recipient and Delivery (Two columns)
                            ComposeRecipientDelivery(innerColumn, data);
                            
                            // 4. Line Items Table
                            ComposeLineItems(innerColumn, data);
                            
                            // 5. Totals Section
                            ComposeTotals(innerColumn, data);
                            
                            // 6. Declaration and Terms
                            ComposeDeclarationAndTerms(innerColumn, data);
                            
                            // 7. E-Invoice and Signature
                            ComposeEInvoiceAndSignature(innerColumn, data);
                        });
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateBatchTaxInvoicePdf(List<TaxInvoicePrintDto> invoices)
    {
        var document = Document.Create(container =>
        {
            foreach (var invoice in invoices)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(10);
                    
                    page.Content().Column(column =>
                    {
                        column.Item().Border(2).BorderColor("#000000").Padding(2)
                            .Border(1).BorderColor("#000000").Padding(5)
                            .Column(innerColumn =>
                            {
                                ComposeHeader(innerColumn, invoice);
                                ComposeInvoiceHeader(innerColumn, invoice);
                                ComposeRecipientDelivery(innerColumn, invoice);
                                ComposeLineItems(innerColumn, invoice);
                                ComposeTotals(innerColumn, invoice);
                                ComposeDeclarationAndTerms(innerColumn, invoice);
                                ComposeEInvoiceAndSignature(innerColumn, invoice);
                            });
                    });
                });
            }
        });

        return document.GeneratePdf();
    }

    #region Header Section
    
    private void ComposeHeader(ColumnDescriptor column, TaxInvoicePrintDto data)
    {
        column.Item().Column(headerColumn =>
        {
            // Title and Copy Type
            headerColumn.Item().Row(row =>
            {
                row.RelativeItem().Text("Tax Invoice").Bold().FontSize(16).AlignCenter();
                row.ConstantItem(80).Text(GetCopyTypeName(data.CopyType)).FontSize(10).AlignRight();
            });
            
            // Legal text
            headerColumn.Item().PaddingTop(2).Text(
                "(issued under section 31 of central goods & service tax act 2017 and maharashtra state goods & service tax act 2017)")
                .FontSize(7).AlignCenter();
            
            // Company name
            headerColumn.Item().PaddingTop(3).Text(data.Company.CompanyName)
                .Bold().FontSize(12).AlignCenter();
            
            // Company address
            headerColumn.Item().PaddingTop(2).Text(data.Company.FullAddress)
                .FontSize(9).AlignCenter();
        });
        
        column.Item().PaddingTop(5).LineHorizontal(1).LineColor("#000000");
    }
    
    #endregion

    #region Invoice Header (Two Columns)
    
    private void ComposeInvoiceHeader(ColumnDescriptor column, TaxInvoicePrintDto data)
    {
        column.Item().PaddingTop(5).Row(row =>
        {
            // Left column
            row.RelativeItem().Column(leftColumn =>
            {
                AddLabelValue(leftColumn, "Date Of Invoice", $": {data.InvoiceHeader.DateOfInvoice:dd/MM/yyyy}");
                AddLabelValue(leftColumn, "Invoice Serial No", $": {data.InvoiceHeader.InvoiceSerialNo}");
                AddLabelValue(leftColumn, "GSTIN No", $": {data.InvoiceHeader.GstinNo}");
                AddLabelValue(leftColumn, "E-Way Bill No", $": {data.InvoiceHeader.EWayBillNo}");
            });
            
            // Right column
            row.RelativeItem().Column(rightColumn =>
            {
                AddLabelValue(rightColumn, "Transporatation Mode", $": {data.InvoiceHeader.TransporatationMode}");  // Keep typo
                AddLabelValue(rightColumn, "Vehicle No", $": {data.InvoiceHeader.VehicleNo}");
                AddLabelValue(rightColumn, "PO No.", $": {data.InvoiceHeader.PoNo}");
                AddLabelValue(rightColumn, "Date & Time of Supply", $": {data.InvoiceHeader.DateAndTimeOfSupply:dd/MM/yyyy  HH:mm}");
                AddLabelValue(rightColumn, "Place Of Supply", $": {data.InvoiceHeader.PlaceOfSupply}");
            });
        });
        
        column.Item().PaddingTop(5).LineHorizontal(1).LineColor("#000000");
    }
    
    #endregion

    #region Recipient and Delivery (Two Columns)
    
    private void ComposeRecipientDelivery(ColumnDescriptor column, TaxInvoicePrintDto data)
    {
        column.Item().PaddingTop(5).Row(row =>
        {
            // Left: Details Of Recipient
            row.RelativeItem().Border(1).BorderColor("#000000").Padding(5).Column(recipientColumn =>
            {
                recipientColumn.Item().Text("Details Of Recipient").Bold().FontSize(10);
                recipientColumn.Item().PaddingTop(3).Column(col =>
                {
                    AddLabelValue(col, "Name", $": {data.Recipient.Name}");
                    AddLabelValue(col, "Address", $": {data.Recipient.Address}");
                    AddLabelValue(col, "State Name", $": {data.Recipient.StateName}");
                    AddLabelValue(col, "State Code", $": {data.Recipient.StateCode}");
                    AddLabelValue(col, "GSTIN No", $": {data.Recipient.GstinNo}");
                });
            });
            
            // Right: Details Of Delivery
            row.RelativeItem().Border(1).BorderColor("#000000").Padding(5).Column(deliveryColumn =>
            {
                deliveryColumn.Item().Text("Details Of Delivery").Bold().FontSize(10);
                deliveryColumn.Item().PaddingTop(3).Column(col =>
                {
                    AddLabelValue(col, "Name", $": {data.Delivery.Name}");
                    AddLabelValue(col, "Address", $": {data.Delivery.Address}");
                    AddLabelValue(col, "State Name", $": {data.Delivery.StateName}");
                    AddLabelValue(col, "State Code", $": {data.Delivery.StateCode}");
                    AddLabelValue(col, "GSTIN No", $": {data.Delivery.GstinNo}");
                });
            });
        });
    }
    
    #endregion

    #region Line Items Table
    
    private void ComposeLineItems(ColumnDescriptor column, TaxInvoicePrintDto data)
    {
        column.Item().PaddingTop(5).Table(table =>
        {
            // Define columns
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30);  // Sr. No
                columns.RelativeColumn(4);    // Description
                columns.ConstantColumn(60);  // HSN/SAC
                columns.ConstantColumn(40);  // UOM
                columns.ConstantColumn(50);  // Qty
                columns.ConstantColumn(60);  // Rate/Unit
                columns.ConstantColumn(70);  // Taxable Value
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("Sr. No").Bold().FontSize(8);
                header.Cell().Element(CellStyle).Text("Description Of Goods Or Services").Bold().FontSize(8);
                header.Cell().Element(CellStyle).Text("HSN/SAC").Bold().FontSize(8);
                header.Cell().Element(CellStyle).Text("UOM").Bold().FontSize(8);
                header.Cell().Element(CellStyle).Text("Qty").Bold().FontSize(8);
                header.Cell().Element(CellStyle).Text("Rate/Unit").Bold().FontSize(8);
                header.Cell().Element(CellStyle).Text("Taxable Value").Bold().FontSize(8);
            });

            // Data rows
            foreach (var item in data.LineItems)
            {
                table.Cell().Element(CellStyle).Text(item.SrNo.ToString()).FontSize(8);
                table.Cell().Element(CellStyle).Text(item.DescriptionOfGoodsOrServices).FontSize(8);
                table.Cell().Element(CellStyle).Text(item.HsnSac).FontSize(8);
                table.Cell().Element(CellStyle).Text(item.Uom).FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text(FormatIndianNumber(item.Qty)).FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text(FormatIndianNumber(item.RatePerUnit)).FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text(FormatIndianNumber(item.TaxableValue)).FontSize(8);
            }
        });
    }
    
    #endregion

    #region Totals Section
    
    private void ComposeTotals(ColumnDescriptor column, TaxInvoicePrintDto data)
    {
        column.Item().PaddingTop(5).Column(totalsColumn =>
        {
            // Less/Add rows
            AddTotalRow(totalsColumn, "Less:", "Discount", data.Totals.Discount);
            AddTotalRow(totalsColumn, "Add:", "Packing & Forwarding Charges", data.Totals.PackingAndForwardingCharges);
            AddTotalRow(totalsColumn, "Add:", "Frieght & Insurance", data.Totals.FrieghtAndInsurance);  // Keep typo
            AddTotalRow(totalsColumn, "Add:", "Other Charges", data.Totals.OtherCharges);
            
            // Taxable Value
            AddTotalRow(totalsColumn, "", "Taxable Value", data.Totals.TaxableValue, isBold: true);
            
            // Taxes
            AddTotalRow(totalsColumn, "", $"Central Tax @ {data.Totals.CentralTaxPercentage:F2} %", data.Totals.CentralTaxAmount);
            AddTotalRow(totalsColumn, "", $"State/Union Territory Tax @ {data.Totals.StateUnionTerritoryTaxPercentage:F2} %", data.Totals.StateUnionTerritoryTaxAmount);
            AddTotalRow(totalsColumn, "", $"Integrated Tax @ {data.Totals.IntegratedTaxPercentage:F2} %", data.Totals.IntegratedTaxAmount);
        });
        
        // Grand Total row with border
        column.Item().PaddingTop(3).Border(1).BorderColor("#000000").Row(row =>
        {
            row.RelativeItem().Padding(5).Text(data.Totals.AmountInWords).FontSize(10).Bold();
            row.ConstantItem(100).Padding(5).AlignRight().Text(FormatIndianNumber(data.Totals.GrandTotal)).FontSize(12).Bold();
        });
    }
    
    #endregion

    #region Declaration and Terms
    
    private void ComposeDeclarationAndTerms(ColumnDescriptor column, TaxInvoicePrintDto data)
    {
        column.Item().PaddingTop(5).Column(declarationColumn =>
        {
            // Declaration
            declarationColumn.Item().Text(data.Declaration).FontSize(7).Italic();
            
            // Terms & Conditions
            if (data.TermsAndConditions?.Any() == true)
            {
                declarationColumn.Item().PaddingTop(5).Text("Terms And Conditions:").Bold().FontSize(9);
                foreach (var term in data.TermsAndConditions)
                {
                    declarationColumn.Item().PaddingTop(2).Text(term).FontSize(8);
                }
            }
        });
    }
    
    #endregion

    #region E-Invoice and Signature
    
    private void ComposeEInvoiceAndSignature(ColumnDescriptor column, TaxInvoicePrintDto data)
    {
        column.Item().PaddingTop(10).Row(row =>
        {
            // Left: E-Invoice with QR Code
            if (data.EInvoice != null && !string.IsNullOrEmpty(data.EInvoice.Irn))
            {
                row.RelativeItem().Column(eInvoiceColumn =>
                {
                    // QR Code
                    var qrBytes = GenerateQRCode(data.EInvoice.Irn, 5);
                    eInvoiceColumn.Item().Width(100).Height(100).Image(qrBytes);
                    
                    // IRN, Ack No, Ack Date
                    eInvoiceColumn.Item().PaddingTop(5).Text($"IRN:- {data.EInvoice.Irn}").FontSize(7);
                    eInvoiceColumn.Item().Text($"Ack No:- {data.EInvoice.AckNo}").FontSize(7);
                    if (data.EInvoice.AckDate.HasValue)
                    {
                        eInvoiceColumn.Item().Text($"Ack Date:- {data.EInvoice.AckDate:yyyy-MM-dd HH:mm:ss}").FontSize(7);
                    }
                });
            }
            else
            {
                row.RelativeItem().Text("");  // Empty space if no E-Invoice
            }
            
            // Right: Signature - positioned at bottom right
            row.RelativeItem().AlignRight().AlignBottom().Column(signatureColumn =>
            {
                signatureColumn.Item().PaddingTop(60).AlignRight().Text("Signature / Digital Signature of").FontSize(9);
                signatureColumn.Item().AlignRight().Text("Authorised Signatory").FontSize(9);
            });
        });
    }
    
    #endregion

    #region Helper Methods
    
    private void AddLabelValue(ColumnDescriptor column, string label, string value)
    {
        column.Item().PaddingBottom(2).Row(row =>
        {
            row.ConstantItem(120).Text(label).FontSize(9);
            row.RelativeItem().Text(value).FontSize(9);
        });
    }
    
    private void AddTotalRow(ColumnDescriptor column, string prefix, string label, decimal amount, bool isBold = false)
    {
        column.Item().PaddingBottom(2).Row(row =>
        {
            row.ConstantItem(40).Text(prefix).FontSize(9);
            row.RelativeItem().Text(label).FontSize(9);
            
            var text = row.ConstantItem(100).AlignRight().Text(FormatIndianNumber(amount)).FontSize(9);
            if (isBold) text.Bold();
        });
    }
    
    /// <summary>
    /// Formats a number with Indian comma notation (1,00,000.00)
    /// </summary>
    private static string FormatIndianNumber(decimal number)
    {
        // Format with 2 decimal places
        string formatted = number.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        
        // Split into integer and decimal parts
        string[] parts = formatted.Split('.');
        string integerPart = parts[0];
        string decimalPart = parts.Length > 1 ? parts[1] : "00";
        
        // Add commas for Indian numbering system
        if (integerPart.Length > 3)
        {
            // Last 3 digits
            string lastThree = integerPart.Substring(integerPart.Length - 3);
            string remaining = integerPart.Substring(0, integerPart.Length - 3);
            
            // Add commas every 2 digits for the remaining part
            string result = "";
            int count = 0;
            for (int i = remaining.Length - 1; i >= 0; i--)
            {
                if (count == 2)
                {
                    result = "," + result;
                    count = 0;
                }
                result = remaining[i] + result;
                count++;
            }
            
            return result + "," + lastThree + "." + decimalPart;
        }
        
        return integerPart + "." + decimalPart;
    }
    
    private static IContainer CellStyle(IContainer container)
    {
        return container.Border(0.5f).BorderColor("#E0E0E0").Padding(3);
    }
    
    private static string GetCopyTypeName(InvoiceCopyType copyType)
    {
        return copyType switch
        {
            InvoiceCopyType.Original => "Original",
            InvoiceCopyType.Duplicate => "Duplicate",
            InvoiceCopyType.Triplicate => "Triplicate",
            _ => "Copy"
        };
    }
    
    public byte[] GenerateQRCode(string text, int pixelsPerModule = 20)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(pixelsPerModule);
    }
    
    #endregion
}
