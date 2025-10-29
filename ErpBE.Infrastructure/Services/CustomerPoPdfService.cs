using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ErpBE.Infrastructure.Services;

/// <summary>
/// Service for generating Customer PO (Sales Order) PDF documents using QuestPDF
/// MATCHES EXACT FORMAT from D:\Invoice\CustomerPO_page-0001.jpg
/// </summary>
public class CustomerPoPdfService : ICustomerPoPdfService
{
    public CustomerPoPdfService()
    {
        // Set QuestPDF license (Community License)
        QuestPDF.Settings.License = LicenseType.Community;
        // Enable debugging for layout issues
        QuestPDF.Settings.EnableDebugging = true;
    }

    public byte[] GenerateCustomerPoPdf(CustomerPoPrintDto data)
    {
        // Generate only single page (no copy type variations)
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(15);
                
                page.Content().Border(2).BorderColor("#000000").Padding(8)
                    .Column(column =>
                    {
                        // 1. Header (Title + Form Number)
                        ComposeHeader(column, data);
                        
                        // 2. Company and PO Info Section
                        ComposeCompanyAndPoInfo(column, data);
                        
                        // 3. Customer Section
                        ComposeCustomerSection(column, data);
                        
                        // 4. Line Items Table
                        ComposeLineItems(column, data);
                        
                        // 5. Footer (Totals, Terms, Signature)
                        ComposeFooter(column, data);
                        
                        // 6. Company Details
                        ComposeCompanyDetails(column, data);
                    });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateBatchCustomerPoPdf(List<CustomerPoPrintDto> pos)
    {
        var document = Document.Create(container =>
        {
            foreach (var po in pos)
            {
                // Generate single page per PO (no copy type variations)
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(15);
                    
                    page.Content().Border(2).BorderColor("#000000").Padding(8)
                        .Column(column =>
                        {
                            ComposeHeader(column, po);
                            ComposeCompanyAndPoInfo(column, po);
                            ComposeCustomerSection(column, po);
                            ComposeLineItems(column, po);
                            ComposeFooter(column, po);
                            ComposeCompanyDetails(column, po);
                        });
                });
            }
        });

        return document.GeneratePdf();
    }

    #region Header Section
    
    private void ComposeHeader(ColumnDescriptor column, CustomerPoPrintDto data)
    {
        column.Item().Row(row =>
        {
            // Title: "Sales Order" (centered, underlined)
            row.RelativeItem().Column(col =>
            {
                col.Item().AlignCenter().Text("Sales Order")
                    .Bold().FontSize(16).Underline();
            });
            
            // Form Number (top-right box)
            row.ConstantItem(150).Border(1).BorderColor("#000000").Padding(5)
                .Text(data.FormNumber ?? "FORM NO : MK/F/05,REV.0")
                .FontSize(8);
        });
    }
    
    #endregion

    #region Company and PO Info Section
    
    private void ComposeCompanyAndPoInfo(ColumnDescriptor column, CustomerPoPrintDto data)
    {
        column.Item().PaddingTop(3).Border(1).BorderColor("#000000")
            .Row(row =>
            {
                // Left: Company Info
                row.RelativeItem().BorderRight(1).BorderColor("#000000").Padding(3)
                    .Column(col =>
                    {
                        col.Item().Text(data.Company.CompanyName).Bold().FontSize(10);
                        col.Item().PaddingTop(2).Text(data.Company.FullAddress).FontSize(7);
                    });
                
                // Right: PO Number and Date
                row.ConstantItem(160).Padding(3).Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Sale Order No.").FontSize(9).Bold();
                        r.ConstantItem(60).Text(data.PoHeader.SaleOrderNo.ToString()).FontSize(9).AlignRight();
                    });
                    
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.RelativeItem().Text("Sale Order Date").FontSize(9).Bold();
                        r.ConstantItem(80).Text(data.PoHeader.SaleOrderDate.ToString("dd MMM yyyy")).FontSize(9).AlignRight();
                    });
                });
            });
    }
    
    #endregion

    #region Customer Section
    
    private void ComposeCustomerSection(ColumnDescriptor column, CustomerPoPrintDto data)
    {
        column.Item().PaddingTop(3).Border(1).BorderColor("#000000")
            .Row(row =>
            {
                // Left: Invoice To
                row.RelativeItem().BorderRight(1).BorderColor("#000000").Padding(3)
                    .Column(col =>
                    {
                        col.Item().Text("Invoice To,").FontSize(8).Italic();
                        col.Item().PaddingTop(2).Text(data.Customer.Name).Bold().FontSize(9);
                        col.Item().PaddingTop(1).Text(data.Customer.Address).FontSize(7);
                        
                        if (!string.IsNullOrEmpty(data.PoHeader.PoNo))
                        {
                            col.Item().PaddingTop(3).Row(r =>
                            {
                                r.AutoItem().Text("PO No").FontSize(8).Bold();
                                r.AutoItem().PaddingLeft(5).Text("-  ").FontSize(8);
                                r.AutoItem().Text(data.PoHeader.PoNo).FontSize(8);
                            });
                        }
                        
                        if (data.PoHeader.PoDate.HasValue)
                        {
                            col.Item().PaddingTop(1).Row(r =>
                            {
                                r.AutoItem().Text("PO Date").FontSize(8).Bold();
                                r.AutoItem().PaddingLeft(5).Text("-  ").FontSize(8);
                                r.AutoItem().Text(data.PoHeader.PoDate.Value.ToString("dd/MM/yyyy")).FontSize(8);
                            });
                        }
                    });
                
                // Right: Consignee and Transport
                row.ConstantItem(160).Padding(3).Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Consignee :").FontSize(8);
                            r.RelativeItem().Text(data.Consignee ?? "").FontSize(8);
                        });
                        
                        col.Item().PaddingTop(25).Row(r =>
                        {
                            r.RelativeItem().Text("Transport Through :").FontSize(8);
                        });
                        col.Item().Text(data.TransportThrough ?? "").FontSize(8);
                    });
            });
    }
    
    #endregion

    #region Line Items Table
    
    private void ComposeLineItems(ColumnDescriptor column, CustomerPoPrintDto data)
    {
        column.Item().Border(1).BorderColor("#000000").Table(table =>
        {
            // Define columns - using relative columns to avoid overflow
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(35);    // Sr. No
                columns.RelativeColumn(4);     // Item Name
                columns.ConstantColumn(50);    // Qty
                columns.ConstantColumn(40);    // Unit
                columns.ConstantColumn(60);    // Rate
                columns.ConstantColumn(70);    // Amount
            });

            // Header Row
            table.Header(header =>
            {
                header.Cell().Border(1).BorderColor("#000000").Padding(3)
                    .Text("Sr. No").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3)
                    .Text("Item Name").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3)
                    .Text("Qty").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3)
                    .Text("Unit").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3)
                    .Text("Rate").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3)
                    .Text("Amount").FontSize(8).Bold();
            });

            // Data Rows
            foreach (var item in data.LineItems)
            {
                table.Cell().Border(1).BorderColor("#000000").Padding(3)
                    .Text(item.SrNo.ToString()).FontSize(8);
                table.Cell().Border(1).BorderColor("#000000").Padding(3)
                    .Text(item.ItemName).FontSize(7);
                table.Cell().Border(1).BorderColor("#000000").Padding(3)
                    .Text(FormatIndianNumber(item.Qty)).FontSize(8).AlignRight();
                table.Cell().Border(1).BorderColor("#000000").Padding(3)
                    .Text(item.Unit).FontSize(8);
                table.Cell().Border(1).BorderColor("#000000").Padding(3)
                    .Text(FormatIndianNumber(item.Rate)).FontSize(8).AlignRight();
                table.Cell().Border(1).BorderColor("#000000").Padding(3)
                    .Text(FormatIndianNumber(item.Amount)).FontSize(8).AlignRight();
            }

            // Add empty rows if needed (minimum 3 rows for proper spacing)
            int emptyRows = Math.Max(0, 3 - data.LineItems.Count);
            for (int i = 0; i < emptyRows; i++)
            {
                table.Cell().Border(1).BorderColor("#000000").Padding(2).Text("");
                table.Cell().Border(1).BorderColor("#000000").Padding(2).Text("");
                table.Cell().Border(1).BorderColor("#000000").Padding(2).Text("");
                table.Cell().Border(1).BorderColor("#000000").Padding(2).Text("");
                table.Cell().Border(1).BorderColor("#000000").Padding(2).Text("");
                table.Cell().Border(1).BorderColor("#000000").Padding(2).Text("");
            }

            // Total Row - Fixed column spans to match column definition
            table.Cell().ColumnSpan(2).Border(1).BorderColor("#000000").Padding(3)
                .Text("Total Qty").FontSize(8).Bold().AlignRight();
            table.Cell().Border(1).BorderColor("#000000").Padding(3)
                .Text(FormatIndianNumber(data.Totals.TotalQty)).FontSize(8).Bold().AlignRight();
            table.Cell().ColumnSpan(2).Border(1).BorderColor("#000000").Padding(3)
                .Text("Assessable Value").FontSize(8).Bold();
            table.Cell().Border(1).BorderColor("#000000").Padding(3)
                .Text(FormatIndianNumber(data.Totals.AssessableValue)).FontSize(8).Bold().AlignRight();
        });
    }
    
    #endregion

    #region Footer Section
    
    private void ComposeFooter(ColumnDescriptor column, CustomerPoPrintDto data)
    {
        column.Item().PaddingTop(3).Border(1).BorderColor("#000000")
            .Row(row =>
            {
                // Left: Order Amount, Delivery Terms, Narrations
                row.RelativeItem().BorderRight(1).BorderColor("#000000").Padding(3)
                    .Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.AutoItem().Text("Order Amount  :").FontSize(8).Bold();
                            r.RelativeItem().PaddingLeft(5).Text(data.Totals.AmountInWords ?? "").FontSize(7).Italic();
                        });
                        
                        col.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Text("Delivery Terms :").FontSize(8).Bold();
                            r.RelativeItem().PaddingLeft(5).Text(data.DeliveryTerms ?? "").FontSize(7);
                        });
                        
                        col.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Text("Narrations").FontSize(8).Bold();
                        });
                        col.Item().PaddingTop(1).Text(data.Narrations ?? "").FontSize(7);
                    });
                
                // Right: Taxes and Total
                row.ConstantItem(160).Padding(3).Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Central Tax").FontSize(8);
                            r.ConstantItem(50).Text(FormatIndianNumber(data.Totals.CentralTax)).FontSize(8).AlignRight();
                        });
                        
                        col.Item().PaddingTop(2).Row(r =>
                        {
                            r.RelativeItem().Text("State/Union Territory Tax").FontSize(8);
                            r.ConstantItem(50).Text(FormatIndianNumber(data.Totals.StateUnionTerritoryTax)).FontSize(8).AlignRight();
                        });
                        
                        col.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text("Total Amount :").FontSize(8).Bold();
                            r.ConstantItem(50).Text(FormatIndianNumber(data.Totals.TotalAmount)).FontSize(8).Bold().AlignRight();
                        });
                    });
            });
        
        // Signature Section
        column.Item().PaddingTop(5).Border(1).BorderColor("#000000").Padding(5)
            .Column(col =>
            {
                col.Item().AlignRight().Text($"For {data.Company.CompanyName}").FontSize(9);
                col.Item().PaddingTop(20).AlignRight().Text("Authorised Signatory").FontSize(8);
            });
    }
    
    #endregion

    #region Company Details Section
    
    private void ComposeCompanyDetails(ColumnDescriptor column, CustomerPoPrintDto data)
    {
        column.Item().PaddingTop(3).Border(1).BorderColor("#000000").Padding(3)
            .Column(col =>
            {
                col.Item().Text(txt =>
                {
                    txt.Span("Factory").FontSize(8).Bold();
                    txt.Span(" GNO. B/44 PAWANA IND- PREMISES,MIDC,BHOSARI,  PIMPRI,CHINCHWAD MAHARASHTRA,PUNE - 411026, INDIA ").FontSize(7);
                    txt.Span("Tel").FontSize(8).Bold();
                    txt.Span(" 8975002049").FontSize(7);
                });
                
                col.Item().PaddingTop(3).Text(txt =>
                {
                    txt.Span("Head Office").FontSize(8).Bold();
                    txt.Span(" Plot No. A-44/1/2/19-20, Chakan MIDC,Phase II Wasuli, Pune – 410501, Maharashtra, India ").FontSize(7);
                    txt.Span("Tel").FontSize(8).Bold();
                });
                
                col.Item().Text(txt =>
                {
                    txt.Span("Fax No").FontSize(8).Bold();
                    txt.Span(":54  ").FontSize(7);
                    txt.Span("Email").FontSize(8).Bold();
                    txt.Span(": accounts@sunelectrodevices.co.in").FontSize(7);
                    txt.Span("Web").FontSize(8).Bold();
                    txt.Span(":www.sunelectrodevices.co.in").FontSize(7);
                });
            });
    }
    
    #endregion

    #region Helper Methods
    
    /// <summary>
    /// Format number in Indian numbering system (lakhs, crores) with 2 decimal places
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
    
    
    #endregion
}

