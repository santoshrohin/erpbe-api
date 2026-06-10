using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ErpBE.Application.DTOs.LabourChargeInvoice;
using ErpBE.Application.LabourChargeInvoice.Commands;
using ErpBE.Application.LabourChargeInvoice.Queries;
using ErpBE.Application.TaxInvoice.Queries;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Sales.LabourChargeInvoice;

/// <summary>
/// Integration tests for Labour Charge Invoice against a real SQL Server (Testcontainers).
/// LCI reuses ERP_CreateTaxInvoice + ERP_CreateTaxInvoiceDetail (INM_TYPE='OutJWINM'),
/// so stock is tracked in STOCK_LEDGER under doc type 'TAXINV' and is readable via
/// GetTaxInvoiceItemDetailsQuery.
/// </summary>
public class LabourChargeInvoiceIntegrationTests : IntegrationTestBase
{
    private const int CompanyCode  = 1;
    private const int CustomerCode = 1;
    private const int PoCode       = 1;
    private const int ItemCode     = 1;
    private const int Item2Code    = 2;
    private const int UomCode      = 1;

    private async Task<double> GetStockAsync(int itemCode) =>
        (await Mediator.Send(new GetTaxInvoiceItemDetailsQuery
        {
            ItemCode    = itemCode,
            CompanyCode = CompanyCode,
        }))?.StockQuantity ?? 0;

    private CreateLabourChargeInvoiceCommand MakeCreate(
        int itemCode, double qty, byte invoiceType = 0) =>
        new()
        {
            CompanyCode  = CompanyCode,
            CustomerCode = CustomerCode,
            InvoiceDate  = DateTime.Today,
            InvoiceType  = invoiceType,
            Details = new List<CreateLabourChargeInvoiceDetailRequest>
            {
                new()
                {
                    ItemCode        = itemCode,
                    UomCode         = UomCode,
                    CustomerPoCode  = PoCode,
                    InvoiceQuantity = (decimal)qty,
                    Rate            = 100,
                    CgstPercentage  = 9,
                    SgstPercentage  = 9,
                }
            }
        };

    private async Task CleanupAsync(int invoiceCode)
    {
        try
        {
            await Mediator.Send(new DeleteLabourChargeInvoiceCommand
                { InvoiceCode = invoiceCode, CompanyCode = CompanyCode });
        }
        catch { /* already deleted */ }
    }

    // ── CRUD persistence ────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidInvoice_ShouldReturnNonZeroCode()
    {
        var invoice = await Mediator.Send(MakeCreate(ItemCode, 1));
        try
        {
            invoice.InvoiceCode.Should().NotBe(0, "InvoiceCode must be non-zero after insert");
        }
        finally { await CleanupAsync(invoice.InvoiceCode); }
    }

    [Fact]
    public async Task Create_InvoiceNumber_ShouldBeSequential()
    {
        var inv1 = await Mediator.Send(MakeCreate(ItemCode, 1));
        var inv2 = await Mediator.Send(MakeCreate(ItemCode, 1));
        try
        {
            inv2.InvoiceNumber.Should().BeGreaterThan(inv1.InvoiceNumber,
                "invoice numbers must increment with each create");
        }
        finally
        {
            await CleanupAsync(inv1.InvoiceCode);
            await CleanupAsync(inv2.InvoiceCode);
        }
    }

    [Fact]
    public async Task GetById_ExistingInvoice_ShouldReturnDetails()
    {
        var invoice = await Mediator.Send(MakeCreate(ItemCode, 2));
        try
        {
            var fetched = await Mediator.Send(new GetLabourChargeInvoiceByIdQuery
                { InvoiceCode = invoice.InvoiceCode, CompanyCode = CompanyCode });

            fetched.Should().NotBeNull();
            fetched!.InvoiceCode.Should().Be(invoice.InvoiceCode);
            fetched.Details.Should().HaveCount(1);
        }
        finally { await CleanupAsync(invoice.InvoiceCode); }
    }

    [Fact]
    public async Task GetById_NonExistent_ShouldReturnNull()
    {
        var result = await Mediator.Send(new GetLabourChargeInvoiceByIdQuery
            { InvoiceCode = int.MaxValue, CompanyCode = CompanyCode });

        result.Should().BeNull();
    }

    [Fact]
    public async Task Delete_ExistingInvoice_ShouldReturnTrue()
    {
        var invoice = await Mediator.Send(MakeCreate(ItemCode, 1));

        var result = await Mediator.Send(new DeleteLabourChargeInvoiceCommand
            { InvoiceCode = invoice.InvoiceCode, CompanyCode = CompanyCode });

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_AlreadyDeleted_ShouldReturnFalse()
    {
        var invoice = await Mediator.Send(MakeCreate(ItemCode, 1));
        var cmd = new DeleteLabourChargeInvoiceCommand
            { InvoiceCode = invoice.InvoiceCode, CompanyCode = CompanyCode };
        await Mediator.Send(cmd);

        var second = await Mediator.Send(cmd);
        second.Should().BeFalse("deleting an already-deleted LCI must return false");
    }

    // ── Invoice type tests ──────────────────────────────────────────────────

    [Fact]
    public async Task Create_Type0_AsPerBom_ShouldSucceed()
    {
        var invoice = await Mediator.Send(MakeCreate(ItemCode, 1, invoiceType: 0));
        try
        {
            invoice.InvoiceCode.Should().NotBe(0);
            invoice.InvoiceType.Should().Be(0);
        }
        finally { await CleanupAsync(invoice.InvoiceCode); }
    }

    [Fact]
    public async Task Create_Type1_OneToOne_ShouldSucceed()
    {
        var invoice = await Mediator.Send(MakeCreate(ItemCode, 1, invoiceType: 1));
        try
        {
            invoice.InvoiceCode.Should().NotBe(0);
            invoice.InvoiceType.Should().Be(1);
        }
        finally { await CleanupAsync(invoice.InvoiceCode); }
    }

    [Fact]
    public async Task Create_Type2_ReworkInward_ShouldSucceed()
    {
        var invoice = await Mediator.Send(MakeCreate(ItemCode, 1, invoiceType: 2));
        try
        {
            invoice.InvoiceCode.Should().NotBe(0);
        }
        finally { await CleanupAsync(invoice.InvoiceCode); }
    }

    // ── Stock tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldDeductStockFromLedger()
    {
        var before = await GetStockAsync(ItemCode);
        const double qty = 3;

        var invoice = await Mediator.Send(MakeCreate(ItemCode, qty));
        try
        {
            var after = await GetStockAsync(ItemCode);
            (before - after).Should().Be(qty,
                "creating LCI must deduct the invoiced qty from STOCK_LEDGER");
        }
        finally { await CleanupAsync(invoice.InvoiceCode); }
    }

    [Fact]
    public async Task Delete_ShouldRestoreStock()
    {
        var before = await GetStockAsync(ItemCode);
        const double qty = 4;

        var invoice = await Mediator.Send(MakeCreate(ItemCode, qty));
        await Mediator.Send(new DeleteLabourChargeInvoiceCommand
            { InvoiceCode = invoice.InvoiceCode, CompanyCode = CompanyCode });

        var after = await GetStockAsync(ItemCode);
        after.Should().Be(before, "deleting LCI must fully restore stock");
    }

    [Fact]
    public async Task Update_IncreaseQuantity_ShouldDeductAdditionalStock()
    {
        var invoice = await Mediator.Send(MakeCreate(ItemCode, 5));
        var stockAfterCreate = await GetStockAsync(ItemCode);

        try
        {
            await Mediator.Send(new UpdateLabourChargeInvoiceCommand
            {
                InvoiceCode  = invoice.InvoiceCode,
                CompanyCode  = CompanyCode,
                CustomerCode = CustomerCode,
                InvoiceDate  = DateTime.Today,
                InvoiceType  = 0,
                Details = new List<CreateLabourChargeInvoiceDetailRequest>
                {
                    new()
                    {
                        ItemCode       = ItemCode,
                        UomCode        = UomCode,
                        CustomerPoCode = PoCode,
                        InvoiceQuantity = 8,
                        Rate           = 100,
                    }
                }
            });

            var stockAfterUpdate = await GetStockAsync(ItemCode);
            (stockAfterCreate - stockAfterUpdate).Should().Be(3,
                "increasing qty by 3 must deduct 3 more from stock");
        }
        finally { await CleanupAsync(invoice.InvoiceCode); }
    }

    [Fact]
    public async Task Update_DecreaseQuantity_ShouldRestoreStock()
    {
        var invoice = await Mediator.Send(MakeCreate(ItemCode, 8));
        var stockAfterCreate = await GetStockAsync(ItemCode);

        try
        {
            await Mediator.Send(new UpdateLabourChargeInvoiceCommand
            {
                InvoiceCode  = invoice.InvoiceCode,
                CompanyCode  = CompanyCode,
                CustomerCode = CustomerCode,
                InvoiceDate  = DateTime.Today,
                InvoiceType  = 0,
                Details = new List<CreateLabourChargeInvoiceDetailRequest>
                {
                    new()
                    {
                        ItemCode        = ItemCode,
                        UomCode         = UomCode,
                        CustomerPoCode  = PoCode,
                        InvoiceQuantity = 5,
                        Rate            = 100,
                    }
                }
            });

            var stockAfterUpdate = await GetStockAsync(ItemCode);
            (stockAfterUpdate - stockAfterCreate).Should().Be(3,
                "decreasing qty by 3 must restore 3 to stock");
        }
        finally { await CleanupAsync(invoice.InvoiceCode); }
    }

    [Fact]
    public async Task Update_ShouldNotDoubleDeductStock()
    {
        var before = await GetStockAsync(ItemCode);
        var invoice = await Mediator.Send(MakeCreate(ItemCode, 5));

        try
        {
            var updateCmd = new UpdateLabourChargeInvoiceCommand
            {
                InvoiceCode  = invoice.InvoiceCode,
                CompanyCode  = CompanyCode,
                CustomerCode = CustomerCode,
                InvoiceDate  = DateTime.Today,
                InvoiceType  = 0,
                Details = new List<CreateLabourChargeInvoiceDetailRequest>
                {
                    new()
                    {
                        ItemCode        = ItemCode,
                        UomCode         = UomCode,
                        CustomerPoCode  = PoCode,
                        InvoiceQuantity = 5,
                        Rate            = 100,
                    }
                }
            };

            await Mediator.Send(updateCmd);
            await Mediator.Send(updateCmd);

            (before - await GetStockAsync(ItemCode)).Should().Be(5,
                "two identical updates must not double-deduct stock");
        }
        finally { await CleanupAsync(invoice.InvoiceCode); }
    }

    // ── Lock / Unlock ────────────────────────────────────────────────────────

    [Fact]
    public async Task Lock_ThenUnlock_ShouldToggleLockState()
    {
        var invoice = await Mediator.Send(MakeCreate(ItemCode, 1));

        try
        {
            var repo = GetService<ErpBE.Application.Interfaces.ILabourChargeInvoiceRepository>();

            var locked = await repo.LockAsync(invoice.InvoiceCode, CompanyCode, 0);
            locked.Should().BeTrue("LockAsync must succeed for an unlocked invoice");

            var alreadyLocked = await repo.LockAsync(invoice.InvoiceCode, CompanyCode, 0);
            alreadyLocked.Should().BeFalse("LockAsync must return false when already locked");

            var unlocked = await repo.UnlockAsync(invoice.InvoiceCode, CompanyCode);
            unlocked.Should().BeTrue("UnlockAsync must succeed for a locked invoice");

            var alreadyUnlocked = await repo.UnlockAsync(invoice.InvoiceCode, CompanyCode);
            alreadyUnlocked.Should().BeFalse("UnlockAsync must return false when already unlocked");
        }
        finally { await CleanupAsync(invoice.InvoiceCode); }
    }
}
