using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErpBE.Application.TaxInvoice.Commands;
using ErpBE.Application.TaxInvoice.Queries;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Sales
{
    /// <summary>
    /// Stock + pending-quantity consistency tests for Tax Invoice.
    ///
    /// Strategy used by ERP_UpdateTaxInvoice + ERP_CreateTaxInvoiceDetail:
    ///   CREATE  → stock deducted  (insert negative row in STOCK_LEDGER per detail line)
    ///   UPDATE  → old STOCK_LEDGER rows deleted (stock restored), new rows inserted (stock deducted)
    ///   DELETE  → STOCK_LEDGER rows deleted (stock fully restored)
    ///
    /// Each test reads stockQuantity via GetTaxInvoiceItemDetailsQuery before and after
    /// the operation to verify the correct net change.
    /// </summary>
    public class TaxInvoiceStockTests : IntegrationTestBase
    {
        // Reference seeded master codes — must exist in the test DB
        private const int CompanyCode   = 1;
        private const int CustomerCode  = 1;
        private const int PoCode        = 1;
        private const int ItemCode      = 1;
        private const int Item2Code     = 2;
        private const int UomCode       = 1;

        // ── helpers ───────────────────────────────────────────────────────────

        private CreateTaxInvoiceCommand MakeCreateCommand(
            int itemCode, double qty, int? poCode = PoCode) =>
            new()
            {
                CompanyCode  = CompanyCode,
                InvoiceDate  = DateTime.Now,
                CustomerCode = CustomerCode,
                InvoiceType  = 0,
                Type         = "TAXINV",
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new()
                    {
                        ItemCode        = itemCode,
                        UomCode         = UomCode,
                        CustomerPoCode  = poCode,
                        InvoiceQuantity = qty,
                        Rate            = 100,
                        CgstPercentage  = 9,
                        SgstPercentage  = 9,
                    }
                }
            };

        private async Task<double> GetStockAsync(int itemCode) =>
            (await Mediator.Send(new GetTaxInvoiceItemDetailsQuery
            {
                ItemCode    = itemCode,
                CompanyCode = CompanyCode,
            }))?.StockQuantity ?? 0;

        private async Task CleanupAsync(long invoiceCode)
        {
            try
            {
                await Mediator.Send(new DeleteTaxInvoiceCommand
                {
                    InvoiceCode = invoiceCode,
                    CompanyCode = CompanyCode,
                });
            }
            catch { /* ignore — already deleted */ }
        }

        // ── CREATE tests ──────────────────────────────────────────────────────

        [Fact]
        public async Task Create_ShouldDeductStockByInvoiceQuantity()
        {
            var before = await GetStockAsync(ItemCode);
            const double qty = 5;

            var invoice = await Mediator.Send(MakeCreateCommand(ItemCode, qty));
            try
            {
                var after = await GetStockAsync(ItemCode);
                (before - after).Should().Be(qty,
                    "creating an invoice must deduct exactly the invoiced quantity from stock");
            }
            finally { await CleanupAsync(invoice.InvoiceCode); }
        }

        [Fact]
        public async Task Create_MultiLine_ShouldDeductEachItemIndependently()
        {
            var beforeItem1 = await GetStockAsync(ItemCode);
            var beforeItem2 = await GetStockAsync(Item2Code);

            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode  = CompanyCode,
                InvoiceDate  = DateTime.Now,
                CustomerCode = CustomerCode,
                InvoiceType  = 0,
                Type         = "TAXINV",
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new() { ItemCode = ItemCode,  UomCode = UomCode, CustomerPoCode = PoCode, InvoiceQuantity = 3, Rate = 100 },
                    new() { ItemCode = Item2Code, UomCode = UomCode, CustomerPoCode = PoCode, InvoiceQuantity = 7, Rate = 50  },
                }
            };

            var invoice = await Mediator.Send(command);
            try
            {
                (beforeItem1 - await GetStockAsync(ItemCode)).Should().Be(3);
                (beforeItem2 - await GetStockAsync(Item2Code)).Should().Be(7);
            }
            finally { await CleanupAsync(invoice.InvoiceCode); }
        }

        [Fact]
        public async Task Create_Returns_NonZeroInvoiceCode()
        {
            var invoice = await Mediator.Send(MakeCreateCommand(ItemCode, 1));
            try
            {
                // INVOICE_MASTER uses IDENTITY(-2147483648,1) so codes start negative and count up.
                // The regression we guard against is overflow to *zero* (wrong DbType cast).
                invoice.InvoiceCode.Should().NotBe(0,
                    "BIGINT output parameter must not overflow to 0 — negative values are valid (IDENTITY starts at INT_MIN)");
            }
            finally { await CleanupAsync(invoice.InvoiceCode); }
        }

        // ── UPDATE tests ──────────────────────────────────────────────────────

        [Fact]
        public async Task Update_IncreaseQuantity_ShouldDeductAdditionalStock()
        {
            var invoice = await Mediator.Send(MakeCreateCommand(ItemCode, 5));
            var stockAfterCreate = await GetStockAsync(ItemCode);

            try
            {
                // Increase qty from 5 → 8 (delta = 3 more deducted)
                var updateCommand = BuildUpdateCommand(invoice.InvoiceCode, ItemCode, 8);
                await Mediator.Send(updateCommand);

                var stockAfterUpdate = await GetStockAsync(ItemCode);
                (stockAfterCreate - stockAfterUpdate).Should().Be(3,
                    "increasing qty by 3 must deduct 3 more from stock");
            }
            finally { await CleanupAsync(invoice.InvoiceCode); }
        }

        [Fact]
        public async Task Update_DecreaseQuantity_ShouldRestoreStock()
        {
            var invoice = await Mediator.Send(MakeCreateCommand(ItemCode, 8));
            var stockAfterCreate = await GetStockAsync(ItemCode);

            try
            {
                // Decrease qty from 8 → 5 (delta = 3 restored)
                var updateCommand = BuildUpdateCommand(invoice.InvoiceCode, ItemCode, 5);
                await Mediator.Send(updateCommand);

                var stockAfterUpdate = await GetStockAsync(ItemCode);
                (stockAfterUpdate - stockAfterCreate).Should().Be(3,
                    "decreasing qty by 3 must restore 3 back to stock");
            }
            finally { await CleanupAsync(invoice.InvoiceCode); }
        }

        [Fact]
        public async Task Update_ChangeItem_ShouldRestoreOldItemAndDeductNewItem()
        {
            var invoice = await Mediator.Send(MakeCreateCommand(ItemCode, 5));
            var stockItem1After = await GetStockAsync(ItemCode);
            var stockItem2Before = await GetStockAsync(Item2Code);

            try
            {
                // Replace ItemCode → Item2Code, qty = 4
                var updateCommand = BuildUpdateCommand(invoice.InvoiceCode, Item2Code, 4);
                await Mediator.Send(updateCommand);

                var stockItem1Final  = await GetStockAsync(ItemCode);
                var stockItem2Final  = await GetStockAsync(Item2Code);

                // Old item: stock restored by 5
                (stockItem1Final - stockItem1After).Should().Be(5,
                    "old item stock must be fully restored when line item is replaced");

                // New item: stock deducted by 4
                (stockItem2Before - stockItem2Final).Should().Be(4,
                    "new item stock must be deducted by its invoice quantity");
            }
            finally { await CleanupAsync(invoice.InvoiceCode); }
        }

        [Fact]
        public async Task Update_NoChange_ShouldLeaveStockUnchanged()
        {
            var invoice = await Mediator.Send(MakeCreateCommand(ItemCode, 5));
            var stockAfterCreate = await GetStockAsync(ItemCode);

            try
            {
                // Same item, same qty
                var updateCommand = BuildUpdateCommand(invoice.InvoiceCode, ItemCode, 5);
                await Mediator.Send(updateCommand);

                var stockAfterUpdate = await GetStockAsync(ItemCode);
                stockAfterUpdate.Should().Be(stockAfterCreate,
                    "updating with identical quantities must leave stock unchanged");
            }
            finally { await CleanupAsync(invoice.InvoiceCode); }
        }

        [Fact]
        public async Task Update_ShouldNotDoubleDeductStock()
        {
            var before = await GetStockAsync(ItemCode);
            var invoice = await Mediator.Send(MakeCreateCommand(ItemCode, 5));

            try
            {
                // Update twice with qty=5 — stock must end at (before - 5), not (before - 10)
                await Mediator.Send(BuildUpdateCommand(invoice.InvoiceCode, ItemCode, 5));
                await Mediator.Send(BuildUpdateCommand(invoice.InvoiceCode, ItemCode, 5));

                var afterTwoUpdates = await GetStockAsync(ItemCode);
                (before - afterTwoUpdates).Should().Be(5,
                    "two consecutive updates with same qty must not double-deduct stock");
            }
            finally { await CleanupAsync(invoice.InvoiceCode); }
        }

        // ── DELETE tests ──────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_ShouldFullyRestoreStock()
        {
            var before = await GetStockAsync(ItemCode);
            var invoice = await Mediator.Send(MakeCreateCommand(ItemCode, 5));

            await Mediator.Send(new DeleteTaxInvoiceCommand
            {
                InvoiceCode = invoice.InvoiceCode,
                CompanyCode = CompanyCode,
            });

            var after = await GetStockAsync(ItemCode);
            after.Should().Be(before,
                "deleting an invoice must fully restore all stock that was deducted");
        }

        [Fact]
        public async Task Delete_MultiLine_ShouldRestoreAllItems()
        {
            var beforeItem1 = await GetStockAsync(ItemCode);
            var beforeItem2 = await GetStockAsync(Item2Code);

            var command = new CreateTaxInvoiceCommand
            {
                CompanyCode  = CompanyCode,
                InvoiceDate  = DateTime.Now,
                CustomerCode = CustomerCode,
                InvoiceType  = 0,
                Type         = "TAXINV",
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new() { ItemCode = ItemCode,  UomCode = UomCode, CustomerPoCode = PoCode, InvoiceQuantity = 3, Rate = 100 },
                    new() { ItemCode = Item2Code, UomCode = UomCode, CustomerPoCode = PoCode, InvoiceQuantity = 7, Rate = 50  },
                }
            };

            var invoice = await Mediator.Send(command);
            await Mediator.Send(new DeleteTaxInvoiceCommand
            {
                InvoiceCode = invoice.InvoiceCode,
                CompanyCode = CompanyCode,
            });

            (await GetStockAsync(ItemCode)).Should().Be(beforeItem1);
            (await GetStockAsync(Item2Code)).Should().Be(beforeItem2);
        }

        // ── pending-quantity tests ────────────────────────────────────────────

        [Fact]
        public async Task Create_ShouldReturnPendingQuantityForPO()
        {
            // PO pending quantity is visible via getPOsByItemCustomer query.
            // We don't require a specific PoCode — just that the call succeeds and returns a list.
            var pos = await Mediator.Send(new GetTaxInvoicePOsQuery
            {
                ItemCode     = ItemCode,
                CustomerCode = CustomerCode,
                CompanyCode  = CompanyCode,
                InvoiceCode  = null,
            });

            // The call must not throw and must return a non-null list (empty is acceptable)
            pos.Should().NotBeNull("GetTaxInvoicePOsQuery must return a list (may be empty if no open POs)");
            if (pos!.Any())
                pos.Should().AllSatisfy(p => p.PendingQuantity.Should().BeGreaterThanOrEqualTo(0));
        }

        [Fact]
        public async Task Create_ShouldReducePendingQuantityByInvoicedAmount()
        {
            // Use the first available PO with pending quantity; skip if none exist.
            var posBefore = await Mediator.Send(new GetTaxInvoicePOsQuery
            {
                ItemCode     = ItemCode,
                CustomerCode = CustomerCode,
                CompanyCode  = CompanyCode,
                InvoiceCode  = null,
            });

            var availablePo = posBefore?.FirstOrDefault(p => p.PendingQuantity > 0);
            if (availablePo == null)
            {
                // No open PO available in this test DB — skip rather than fail
                return;
            }

            var pendingBefore = availablePo.PendingQuantity;
            const double qty = 1; // invoice 1 unit against the PO

            var cmd = MakeCreateCommand(ItemCode, qty, availablePo.PoCode);
            var invoice = await Mediator.Send(cmd);
            try
            {
                var posAfter = await Mediator.Send(new GetTaxInvoicePOsQuery
                {
                    ItemCode     = ItemCode,
                    CustomerCode = CustomerCode,
                    CompanyCode  = CompanyCode,
                    InvoiceCode  = null,
                });
                var pendingAfter = posAfter?.FirstOrDefault(p => p.PoCode == availablePo.PoCode)?.PendingQuantity ?? 0;

                (pendingBefore - pendingAfter).Should().Be(qty,
                    "invoicing against a PO must reduce that PO's pending quantity");
            }
            finally { await CleanupAsync(invoice.InvoiceCode); }
        }

        // ── edge cases ────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_ZeroQuantity_ShouldNotChangeStock()
        {
            var before = await GetStockAsync(ItemCode);

            // Zero-qty line — validation should reject this in the handler
            // If it doesn't throw, stock must still be unchanged
            long? createdCode = null;
            try
            {
                var invoice = await Mediator.Send(MakeCreateCommand(ItemCode, 0));
                createdCode = invoice.InvoiceCode;
            }
            catch { /* expected — handler should reject qty=0 */ }
            finally
            {
                if (createdCode.HasValue) await CleanupAsync(createdCode.Value);
            }

            var after = await GetStockAsync(ItemCode);
            after.Should().Be(before, "zero-quantity invoice must not alter stock");
        }

        [Fact]
        public async Task Delete_AlreadyDeletedInvoice_ShouldReturnFalse()
        {
            var invoice = await Mediator.Send(MakeCreateCommand(ItemCode, 1));
            var deleteCmd = new DeleteTaxInvoiceCommand
            {
                InvoiceCode = invoice.InvoiceCode,
                CompanyCode = CompanyCode,
            };

            await Mediator.Send(deleteCmd);
            var secondDelete = await Mediator.Send(deleteCmd);

            secondDelete.Should().BeFalse("deleting an already-deleted invoice must return false");
        }

        [Fact]
        public async Task Create_WithSupplementaryFlag_ShouldStillDeductStock()
        {
            var before = await GetStockAsync(ItemCode);
            var command = MakeCreateCommand(ItemCode, 3);
            command.IsSupplementary = true;

            var invoice = await Mediator.Send(command);
            try
            {
                var after = await GetStockAsync(ItemCode);
                (before - after).Should().Be(3,
                    "supplementary invoice must also deduct stock (same as regular invoice)");
            }
            finally { await CleanupAsync(invoice.InvoiceCode); }
        }

        // ── BIGINT id regression ──────────────────────────────────────────────

        [Fact]
        public async Task Create_InvoiceCode_MustBeValidLong()
        {
            var invoice = await Mediator.Send(MakeCreateCommand(ItemCode, 1));
            try
            {
                // INVOICE_MASTER uses IDENTITY(-2147483648,1): codes start at INT_MIN and count up.
                // The regression we guard against is a value of 0 (wrong DbType cast corrupts the code).
                invoice.InvoiceCode.Should().NotBe(0,
                    "InvoiceCode must be a non-zero long — zero means the INT OUTPUT parameter overflowed");
                invoice.InvoiceCode.Should().BeLessThanOrEqualTo(long.MaxValue);

                // Must be retrievable by the returned code
                var fetched = await Mediator.Send(new GetTaxInvoiceByIdQuery
                    { InvoiceCode = invoice.InvoiceCode, CompanyCode = CompanyCode });
                fetched.Should().NotBeNull("invoice must be retrievable by its code");
            }
            finally { await CleanupAsync(invoice.InvoiceCode); }
        }

        // ── helper ────────────────────────────────────────────────────────────

        private static UpdateTaxInvoiceCommand BuildUpdateCommand(
            long invoiceCode, int itemCode, double qty) =>
            new()
            {
                InvoiceCode  = invoiceCode,
                CompanyCode  = CompanyCode,
                InvoiceDate  = DateTime.Now,
                CustomerCode = CustomerCode,
                InvoiceType  = 0,
                Type         = "TAXINV",
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new()
                    {
                        ItemCode        = itemCode,
                        UomCode         = UomCode,
                        CustomerPoCode  = PoCode,
                        InvoiceQuantity = qty,
                        Rate            = 100,
                        CgstPercentage  = 9,
                        SgstPercentage  = 9,
                    }
                }
            };
    }
}
