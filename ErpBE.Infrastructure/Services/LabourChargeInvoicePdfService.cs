using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ErpBE.Infrastructure.Services;

public class LabourChargeInvoicePdfService : ILabourChargeInvoicePdfService
{
    public LabourChargeInvoicePdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateLabourChargeInvoicePdf(LabourChargeInvoicePrintDto data)
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
                        ComposeTotals(col, data);
                        ComposeSignature(col);
                    });
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(ColumnDescriptor col, LabourChargeInvoicePrintDto data)
    {
        col.Item().AlignCenter().Text(data.CompanyName).FontSize(14).Bold();
        col.Item().AlignCenter().Text(data.CompanyAddress).FontSize(8);
        if (!string.IsNullOrEmpty(data.CompanyGstin))
            col.Item().AlignCenter().Text($"GSTIN: {data.CompanyGstin}").FontSize(8);

        col.Item().PaddingVertical(4).AlignCenter().Text("LABOUR CHARGE INVOICE").FontSize(12).Bold().Underline();

        col.Item().Row(row =>
        {
            row.RelativeItem().Text($"Invoice No: {data.InvoiceNumber}").FontSize(9);
            row.RelativeItem().AlignRight().Text($"Date: {data.InvoiceDate:dd/MM/yyyy}").FontSize(9);
        });
    }

    private static void ComposeParties(ColumnDescriptor col, LabourChargeInvoicePrintDto data)
    {
        col.Item().PaddingTop(6).BorderTop(1).BorderColor("#000000")
            .Column(inner =>
            {
                inner.Item().Text("To,").FontSize(9).Bold();
                inner.Item().Text(data.CustomerName).FontSize(9).Bold();
                inner.Item().Text(data.CustomerAddress).FontSize(8);
                if (!string.IsNullOrEmpty(data.CustomerGstin))
                    inner.Item().Text($"GSTIN: {data.CustomerGstin}").FontSize(8);
            });

        if (!string.IsNullOrWhiteSpace(data.VehicleNumber) || !string.IsNullOrWhiteSpace(data.LrNumber))
        {
            col.Item().PaddingTop(4).Row(row =>
            {
                if (!string.IsNullOrWhiteSpace(data.VehicleNumber))
                    row.RelativeItem().Text($"Vehicle No: {data.VehicleNumber}").FontSize(8);
                if (!string.IsNullOrWhiteSpace(data.LrNumber))
                    row.RelativeItem().AlignRight().Text($"LR No: {data.LrNumber}").FontSize(8);
            });
        }
    }

    private static void ComposeLineItems(ColumnDescriptor col, LabourChargeInvoicePrintDto data)
    {
        col.Item().PaddingTop(6).Border(1).BorderColor("#000000").Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(30);
                cols.RelativeColumn(3);
                cols.ConstantColumn(50);
                cols.ConstantColumn(40);
                cols.ConstantColumn(60);
                cols.ConstantColumn(70);
                cols.ConstantColumn(80);
            });

            table.Header(header =>
            {
                header.Cell().Border(1).BorderColor("#000000").Padding(3).Text("Sr.").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3).Text("Description").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3).Text("HSN").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3).Text("UOM").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3).Text("Qty").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3).Text("Rate").FontSize(8).Bold();
                header.Cell().Border(1).BorderColor("#000000").Padding(3).Text("Amount").FontSize(8).Bold();
            });

            foreach (var item in data.LineItems)
            {
                table.Cell().Border(1).BorderColor("#000000").Padding(3).Text(item.SrNo.ToString()).FontSize(8);
                table.Cell().Border(1).BorderColor("#000000").Padding(3).Text(item.ItemName).FontSize(8);
                table.Cell().Border(1).BorderColor("#000000").Padding(3).Text(item.HsnCode ?? "").FontSize(8);
                table.Cell().Border(1).BorderColor("#000000").Padding(3).Text(item.Uom).FontSize(8);
                table.Cell().Border(1).BorderColor("#000000").Padding(3).Text(item.Qty.ToString("N3")).FontSize(8).AlignRight();
                table.Cell().Border(1).BorderColor("#000000").Padding(3).Text(item.Rate.ToString("N2")).FontSize(8).AlignRight();
                table.Cell().Border(1).BorderColor("#000000").Padding(3).Text(item.Amount.ToString("N2")).FontSize(8).AlignRight();
            }

            int emptyRows = Math.Max(0, 3 - data.LineItems.Count);
            for (int i = 0; i < emptyRows; i++)
            {
                for (int j = 0; j < 7; j++)
                    table.Cell().Border(1).BorderColor("#000000").Padding(3).Text("").FontSize(8);
            }
        });
    }

    private static void ComposeTotals(ColumnDescriptor col, LabourChargeInvoicePrintDto data)
    {
        col.Item().PaddingTop(4).AlignRight().Column(totals =>
        {
            if (data.DiscountAmount != 0)
                totals.Item().Row(r =>
                {
                    r.ConstantItem(150).AlignRight().Text("Less: Discount").FontSize(8);
                    r.ConstantItem(80).AlignRight().Text(data.DiscountAmount.ToString("N2")).FontSize(8);
                });

            if (data.PackingAmount != 0)
                totals.Item().Row(r =>
                {
                    r.ConstantItem(150).AlignRight().Text("Add: Packing").FontSize(8);
                    r.ConstantItem(80).AlignRight().Text(data.PackingAmount.ToString("N2")).FontSize(8);
                });

            if (data.FreightCharges != 0)
                totals.Item().Row(r =>
                {
                    r.ConstantItem(150).AlignRight().Text("Add: Freight").FontSize(8);
                    r.ConstantItem(80).AlignRight().Text(data.FreightCharges.ToString("N2")).FontSize(8);
                });

            if (data.OtherAmount != 0)
                totals.Item().Row(r =>
                {
                    r.ConstantItem(150).AlignRight().Text("Add: Other Charges").FontSize(8);
                    r.ConstantItem(80).AlignRight().Text(data.OtherAmount.ToString("N2")).FontSize(8);
                });

            if (data.TcsAmount != 0)
                totals.Item().Row(r =>
                {
                    r.ConstantItem(150).AlignRight().Text($"Add: TCS ({data.TcsPercentage}%)").FontSize(8);
                    r.ConstantItem(80).AlignRight().Text(data.TcsAmount.ToString("N2")).FontSize(8);
                });

            totals.Item().BorderTop(1).BorderColor("#000000").PaddingTop(2).Row(r =>
            {
                r.ConstantItem(150).AlignRight().Text("Grand Total").FontSize(9).Bold();
                r.ConstantItem(80).AlignRight().Text($"₹ {data.GrossAmount:N2}").FontSize(9).Bold();
            });
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
