using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ErpBE.Infrastructure.Services;

public class DeliveryChallanPdfService : IDeliveryChallanPdfService
{
    public DeliveryChallanPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateDeliveryChallanPdf(DeliveryChallanPrintDto data)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(15);
                page.Content().Border(2).BorderColor("#000000").Padding(8)
                    .Column(col =>
                    {
                        ComposeHeader(col, data);
                        ComposeParties(col, data);
                        ComposeLineItems(col, data);
                        ComposeSignature(col);
                    });
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(ColumnDescriptor col, DeliveryChallanPrintDto data)
    {
        col.Item().AlignCenter().Text(data.CompanyName).FontSize(14).Bold();
        col.Item().AlignCenter().Text(data.CompanyAddress).FontSize(8);
        if (!string.IsNullOrEmpty(data.CompanyGstin))
            col.Item().AlignCenter().Text($"GSTIN: {data.CompanyGstin}").FontSize(8);

        col.Item().PaddingVertical(4).AlignCenter().Text("DELIVERY CHALLAN").FontSize(12).Bold().Underline();

        col.Item().Row(row =>
        {
            row.RelativeItem().Text($"Challan No: {data.ChallanNumber}").FontSize(9);
            row.RelativeItem().AlignRight().Text($"Date: {data.ChallanDate:dd/MM/yyyy}").FontSize(9);
        });

        if (!string.IsNullOrWhiteSpace(data.InvoiceNumber))
            col.Item().Text($"Ref Invoice No: {data.InvoiceNumber}").FontSize(8);
    }

    private static void ComposeParties(ColumnDescriptor col, DeliveryChallanPrintDto data)
    {
        col.Item().PaddingTop(6).BorderTop(1).BorderColor("#000000")
            .Column(inner =>
            {
                inner.Item().Text("To,").FontSize(9).Bold();
                inner.Item().Text(data.CustomerName).FontSize(9).Bold();
                inner.Item().Text(data.CustomerAddress).FontSize(8);
            });

        col.Item().PaddingTop(4).Row(row =>
        {
            if (!string.IsNullOrWhiteSpace(data.VehicleNumber))
                row.RelativeItem().Text($"Vehicle No: {data.VehicleNumber}").FontSize(8);
            if (!string.IsNullOrWhiteSpace(data.LrNumber))
                row.RelativeItem().Text($"LR No: {data.LrNumber}").FontSize(8);
            if (!string.IsNullOrWhiteSpace(data.Through))
                row.RelativeItem().AlignRight().Text($"Through: {data.Through}").FontSize(8);
        });

        col.Item().PaddingTop(2).Text($"Returnable: {(data.IsReturnable ? "Yes" : "No")}").FontSize(8);
    }

    private static void ComposeLineItems(ColumnDescriptor col, DeliveryChallanPrintDto data)
    {
        col.Item().PaddingTop(6).Border(1).BorderColor("#000000").Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(30);
                cols.RelativeColumn(3);
                cols.ConstantColumn(50);
                cols.ConstantColumn(80);
                cols.ConstantColumn(80);
                cols.ConstantColumn(60);
            });

            table.Header(header =>
            {
                header.Cell().Border(1).BorderColor("#000000").Padding(3).Text("Sr.").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3).Text("Item Description").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3).Text("UOM").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3).Text("Ordered Qty").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3).Text("Batch No.").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3).Text("No. of Packs").FontSize(8).Bold();
            });

            foreach (var item in data.LineItems)
            {
                table.Cell().Border(1).BorderColor("#000000").Padding(3).Text(item.SrNo.ToString()).FontSize(8);
                table.Cell().Border(1).BorderColor("#000000").Padding(3).Text(item.ItemName).FontSize(8);
                table.Cell().Border(1).BorderColor("#000000").Padding(3).Text(item.Uom).FontSize(8);
                table.Cell().Border(1).BorderColor("#000000").Padding(3).Text(item.OrderedQuantity.ToString("N3")).FontSize(8).AlignRight();
                table.Cell().Border(1).BorderColor("#000000").Padding(3).Text(item.BatchNumber ?? "").FontSize(8);
                table.Cell().Border(1).BorderColor("#000000").Padding(3).Text(item.NumberOfPacks ?? "").FontSize(8);
            }

            int emptyRows = Math.Max(0, 3 - data.LineItems.Count);
            for (int i = 0; i < emptyRows; i++)
            {
                for (int j = 0; j < 6; j++)
                    table.Cell().Border(1).BorderColor("#000000").Padding(3).Text("").FontSize(8);
            }
        });
    }

    private static void ComposeSignature(ColumnDescriptor col)
    {
        col.Item().PaddingTop(20).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("Received By:").FontSize(8);
                c.Item().PaddingTop(20).BorderTop(1).BorderColor("#000000").Text("Signature & Stamp").FontSize(7).Italic();
            });
            row.RelativeItem().AlignRight().Column(c =>
            {
                c.Item().Text("For Company").FontSize(8);
                c.Item().PaddingTop(20).BorderTop(1).BorderColor("#000000").Text("Authorised Signatory").FontSize(7).Italic();
            });
        });
    }
}
