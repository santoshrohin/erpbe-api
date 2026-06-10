using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ErpBE.Application.DeliveryChallan.Commands;
using ErpBE.Application.DeliveryChallan.Queries;
using ErpBE.Application.DTOs;
using ErpBE.Application.TaxInvoice.Queries;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Sales.DeliveryChallan;

/// <summary>
/// Integration tests for Delivery Challan against a real SQL Server (Testcontainers).
/// Stock is tracked via STOCK_LEDGER (same table as Tax Invoice, doc type = 'DCOUT').
/// Stock quantity is read through GetTaxInvoiceItemDetailsQuery which sums all STOCK_LEDGER rows.
/// </summary>
public class DeliveryChallanIntegrationTests : IntegrationTestBase
{
    private const int CompanyCode   = 1;
    private const int CustomerCode  = 1;
    private const int ItemCode      = 1;
    private const int Item2Code     = 2;
    private const int UomCode       = 1;

    private async Task<double> GetStockAsync(int itemCode) =>
        (await Mediator.Send(new GetTaxInvoiceItemDetailsQuery
        {
            ItemCode    = itemCode,
            CompanyCode = CompanyCode,
        }))?.StockQuantity ?? 0;

    private CreateDeliveryChallanCommand MakeCreate(int itemCode, double qty, bool isReturnable = false) =>
        new()
        {
            CompanyCode  = CompanyCode,
            CustomerCode = CustomerCode,
            ChallanDate  = DateTime.Today,
            IsReturnable = isReturnable,
            Details = new List<CreateDeliveryChallanDetailCommand>
            {
                new() { ItemCode = itemCode, UomCode = UomCode, OrderedQuantity = qty }
            }
        };

    private async Task CleanupAsync(int challanCode)
    {
        try
        {
            await Mediator.Send(new DeleteDeliveryChallanCommand
                { ChallanCode = challanCode, CompanyCode = CompanyCode });
        }
        catch { /* already deleted */ }
    }

    // ── CRUD persistence ────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidChallan_ShouldPersistAndReturnNonZeroCode()
    {
        var challan = await Mediator.Send(MakeCreate(ItemCode, 2));
        try
        {
            challan.ChallanCode.Should().NotBe(0, "ChallanCode must be non-zero after insert");
            challan.ChallanNumber.Should().BeGreaterThan(0);
        }
        finally { await CleanupAsync(challan.ChallanCode); }
    }

    [Fact]
    public async Task GetById_ExistingChallan_ShouldReturnDetails()
    {
        var challan = await Mediator.Send(MakeCreate(ItemCode, 3));
        try
        {
            var fetched = await Mediator.Send(new GetDeliveryChallanByIdQuery
                { ChallanCode = challan.ChallanCode, CompanyCode = CompanyCode });

            fetched.Should().NotBeNull();
            fetched!.ChallanCode.Should().Be(challan.ChallanCode);
        }
        finally { await CleanupAsync(challan.ChallanCode); }
    }

    [Fact]
    public async Task GetById_NonExistent_ShouldReturnNull()
    {
        var result = await Mediator.Send(new GetDeliveryChallanByIdQuery
            { ChallanCode = int.MaxValue, CompanyCode = CompanyCode });

        result.Should().BeNull();
    }

    [Fact]
    public async Task Delete_ExistingChallan_ShouldReturnTrue()
    {
        var challan = await Mediator.Send(MakeCreate(ItemCode, 1));

        var result = await Mediator.Send(new DeleteDeliveryChallanCommand
            { ChallanCode = challan.ChallanCode, CompanyCode = CompanyCode });

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_AlreadyDeleted_ShouldReturnFalse()
    {
        var challan = await Mediator.Send(MakeCreate(ItemCode, 1));
        var cmd = new DeleteDeliveryChallanCommand { ChallanCode = challan.ChallanCode, CompanyCode = CompanyCode };
        await Mediator.Send(cmd);

        var second = await Mediator.Send(cmd);
        second.Should().BeFalse("deleting an already-deleted challan must return false");
    }

    [Fact]
    public async Task IsReturnable_Flag_ShouldBePersisted()
    {
        var challan = await Mediator.Send(MakeCreate(ItemCode, 1, isReturnable: true));
        try
        {
            var fetched = await Mediator.Send(new GetDeliveryChallanByIdQuery
                { ChallanCode = challan.ChallanCode, CompanyCode = CompanyCode });

            fetched!.IsReturnable.Should().BeTrue("IsReturnable flag must be persisted");
        }
        finally { await CleanupAsync(challan.ChallanCode); }
    }

    // ── Stock tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldDeductStockByOrderedQuantity()
    {
        var before = await GetStockAsync(ItemCode);
        const double qty = 4;

        var challan = await Mediator.Send(MakeCreate(ItemCode, qty));
        try
        {
            var after = await GetStockAsync(ItemCode);
            (before - after).Should().Be(qty,
                "creating a challan must deduct exactly the ordered quantity from stock");
        }
        finally { await CleanupAsync(challan.ChallanCode); }
    }

    [Fact]
    public async Task Create_MultiLine_ShouldDeductEachItemIndependently()
    {
        var beforeItem1 = await GetStockAsync(ItemCode);
        var beforeItem2 = await GetStockAsync(Item2Code);

        var cmd = new CreateDeliveryChallanCommand
        {
            CompanyCode  = CompanyCode,
            CustomerCode = CustomerCode,
            ChallanDate  = DateTime.Today,
            Details = new List<CreateDeliveryChallanDetailCommand>
            {
                new() { ItemCode = ItemCode,  UomCode = UomCode, OrderedQuantity = 3 },
                new() { ItemCode = Item2Code, UomCode = UomCode, OrderedQuantity = 6 },
            }
        };

        var challan = await Mediator.Send(cmd);
        try
        {
            (beforeItem1 - await GetStockAsync(ItemCode)).Should().Be(3);
            (beforeItem2 - await GetStockAsync(Item2Code)).Should().Be(6);
        }
        finally { await CleanupAsync(challan.ChallanCode); }
    }

    [Fact]
    public async Task Delete_ShouldFullyRestoreStock()
    {
        var before = await GetStockAsync(ItemCode);
        const double qty = 5;

        var challan = await Mediator.Send(MakeCreate(ItemCode, qty));
        await Mediator.Send(new DeleteDeliveryChallanCommand
            { ChallanCode = challan.ChallanCode, CompanyCode = CompanyCode });

        var after = await GetStockAsync(ItemCode);
        after.Should().Be(before, "deleting a challan must fully restore stock");
    }

    [Fact]
    public async Task Delete_MultiLine_ShouldRestoreAllItemsStock()
    {
        var beforeItem1 = await GetStockAsync(ItemCode);
        var beforeItem2 = await GetStockAsync(Item2Code);

        var cmd = new CreateDeliveryChallanCommand
        {
            CompanyCode  = CompanyCode,
            CustomerCode = CustomerCode,
            ChallanDate  = DateTime.Today,
            Details = new List<CreateDeliveryChallanDetailCommand>
            {
                new() { ItemCode = ItemCode,  UomCode = UomCode, OrderedQuantity = 3 },
                new() { ItemCode = Item2Code, UomCode = UomCode, OrderedQuantity = 6 },
            }
        };

        var challan = await Mediator.Send(cmd);
        await Mediator.Send(new DeleteDeliveryChallanCommand
            { ChallanCode = challan.ChallanCode, CompanyCode = CompanyCode });

        (await GetStockAsync(ItemCode)).Should().Be(beforeItem1);
        (await GetStockAsync(Item2Code)).Should().Be(beforeItem2);
    }

    [Fact]
    public async Task Update_IncreaseQuantity_ShouldDeductAdditional()
    {
        var challan = await Mediator.Send(MakeCreate(ItemCode, 5));
        var stockAfterCreate = await GetStockAsync(ItemCode);

        try
        {
            await Mediator.Send(new UpdateDeliveryChallanCommand
            {
                ChallanCode  = challan.ChallanCode,
                CompanyCode  = CompanyCode,
                CustomerCode = CustomerCode,
                ChallanDate  = DateTime.Today,
                Details = new List<UpdateDeliveryChallanDetailCommand>
                {
                    new() { ItemCode = ItemCode, UomCode = UomCode, OrderedQuantity = 8 }
                }
            });

            var stockAfterUpdate = await GetStockAsync(ItemCode);
            (stockAfterCreate - stockAfterUpdate).Should().Be(3,
                "increasing qty by 3 must deduct 3 more from stock");
        }
        finally { await CleanupAsync(challan.ChallanCode); }
    }

    [Fact]
    public async Task Update_DecreaseQuantity_ShouldRestoreStock()
    {
        var challan = await Mediator.Send(MakeCreate(ItemCode, 8));
        var stockAfterCreate = await GetStockAsync(ItemCode);

        try
        {
            await Mediator.Send(new UpdateDeliveryChallanCommand
            {
                ChallanCode  = challan.ChallanCode,
                CompanyCode  = CompanyCode,
                CustomerCode = CustomerCode,
                ChallanDate  = DateTime.Today,
                Details = new List<UpdateDeliveryChallanDetailCommand>
                {
                    new() { ItemCode = ItemCode, UomCode = UomCode, OrderedQuantity = 5 }
                }
            });

            var stockAfterUpdate = await GetStockAsync(ItemCode);
            (stockAfterUpdate - stockAfterCreate).Should().Be(3,
                "decreasing qty by 3 must restore 3 back to stock");
        }
        finally { await CleanupAsync(challan.ChallanCode); }
    }

    [Fact]
    public async Task Update_NoChange_ShouldLeaveStockUnchanged()
    {
        var challan = await Mediator.Send(MakeCreate(ItemCode, 5));
        var stockAfterCreate = await GetStockAsync(ItemCode);

        try
        {
            await Mediator.Send(new UpdateDeliveryChallanCommand
            {
                ChallanCode  = challan.ChallanCode,
                CompanyCode  = CompanyCode,
                CustomerCode = CustomerCode,
                ChallanDate  = DateTime.Today,
                Details = new List<UpdateDeliveryChallanDetailCommand>
                {
                    new() { ItemCode = ItemCode, UomCode = UomCode, OrderedQuantity = 5 }
                }
            });

            var stockAfterUpdate = await GetStockAsync(ItemCode);
            stockAfterUpdate.Should().Be(stockAfterCreate,
                "updating with same qty must leave stock unchanged");
        }
        finally { await CleanupAsync(challan.ChallanCode); }
    }

    [Fact]
    public async Task Update_ShouldNotDoubleDeductStock()
    {
        var before = await GetStockAsync(ItemCode);
        var challan = await Mediator.Send(MakeCreate(ItemCode, 5));

        try
        {
            var updateCmd = new UpdateDeliveryChallanCommand
            {
                ChallanCode  = challan.ChallanCode,
                CompanyCode  = CompanyCode,
                CustomerCode = CustomerCode,
                ChallanDate  = DateTime.Today,
                Details = new List<UpdateDeliveryChallanDetailCommand>
                {
                    new() { ItemCode = ItemCode, UomCode = UomCode, OrderedQuantity = 5 }
                }
            };

            await Mediator.Send(updateCmd);
            await Mediator.Send(updateCmd);

            var afterTwoUpdates = await GetStockAsync(ItemCode);
            (before - afterTwoUpdates).Should().Be(5,
                "two consecutive updates with same qty must not double-deduct stock");
        }
        finally { await CleanupAsync(challan.ChallanCode); }
    }

    [Fact]
    public async Task Update_ChangeItem_ShouldRestoreOldAndDeductNew()
    {
        var challan = await Mediator.Send(MakeCreate(ItemCode, 5));
        var stockItem1After = await GetStockAsync(ItemCode);
        var stockItem2Before = await GetStockAsync(Item2Code);

        try
        {
            await Mediator.Send(new UpdateDeliveryChallanCommand
            {
                ChallanCode  = challan.ChallanCode,
                CompanyCode  = CompanyCode,
                CustomerCode = CustomerCode,
                ChallanDate  = DateTime.Today,
                Details = new List<UpdateDeliveryChallanDetailCommand>
                {
                    new() { ItemCode = Item2Code, UomCode = UomCode, OrderedQuantity = 4 }
                }
            });

            (await GetStockAsync(ItemCode) - stockItem1After).Should().Be(5,
                "old item stock must be fully restored");
            (stockItem2Before - await GetStockAsync(Item2Code)).Should().Be(4,
                "new item stock must be deducted");
        }
        finally { await CleanupAsync(challan.ChallanCode); }
    }

    // ── GetAll / pagination ─────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturnNonNullList()
    {
        var (data, _) = await Mediator.Send(new GetAllDeliveryChallansQuery
        {
            Parameters = new DeliveryChallanQueryParameters
            {
                CompanyCode = CompanyCode,
                PageNumber  = 1,
                PageSize    = 10,
            }
        });

        data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_WithFilter_ShouldReturnMatchingRecords()
    {
        var challan = await Mediator.Send(MakeCreate(ItemCode, 1));
        try
        {
            var (data, _) = await Mediator.Send(new GetAllDeliveryChallansQuery
            {
                Parameters = new DeliveryChallanQueryParameters
                {
                    CompanyCode  = CompanyCode,
                    CustomerCode = CustomerCode,
                    PageNumber   = 1,
                    PageSize     = 100,
                }
            });

            data.Should().NotBeNull();
        }
        finally { await CleanupAsync(challan.ChallanCode); }
    }
}
