using ErpBE.Application.DTOs;
using ErpBE.Application.DTOs.LabourChargeInvoice;
using ErpBE.Application.Interfaces;
using ErpBE.Application.LabourChargeInvoice.Commands;
using ErpBE.Application.LabourChargeInvoice.Handlers;
using ErpBE.Application.LabourChargeInvoice.Queries;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ErpBE.Tests.Sales.LabourChargeInvoice;

public class LabourChargeInvoiceHandlerTests
{
    private readonly ILabourChargeInvoiceRepository              _repository;
    private readonly ILogger<GetAllLabourChargeInvoicesQueryHandler>       _getAllLogger;
    private readonly ILogger<GetLabourChargeInvoiceByIdQueryHandler>       _getByIdLogger;
    private readonly ILogger<CreateLabourChargeInvoiceCommandHandler>      _createLogger;
    private readonly ILogger<UpdateLabourChargeInvoiceCommandHandler>      _updateLogger;
    private readonly ILogger<DeleteLabourChargeInvoiceCommandHandler>      _deleteLogger;
    private readonly ILogger<LockLabourChargeInvoiceCommandHandler>        _lockLogger;
    private readonly ILogger<UnlockLabourChargeInvoiceCommandHandler>      _unlockLogger;

    public LabourChargeInvoiceHandlerTests()
    {
        _repository    = Substitute.For<ILabourChargeInvoiceRepository>();
        _getAllLogger  = Substitute.For<ILogger<GetAllLabourChargeInvoicesQueryHandler>>();
        _getByIdLogger = Substitute.For<ILogger<GetLabourChargeInvoiceByIdQueryHandler>>();
        _createLogger  = Substitute.For<ILogger<CreateLabourChargeInvoiceCommandHandler>>();
        _updateLogger  = Substitute.For<ILogger<UpdateLabourChargeInvoiceCommandHandler>>();
        _deleteLogger  = Substitute.For<ILogger<DeleteLabourChargeInvoiceCommandHandler>>();
        _lockLogger    = Substitute.For<ILogger<LockLabourChargeInvoiceCommandHandler>>();
        _unlockLogger  = Substitute.For<ILogger<UnlockLabourChargeInvoiceCommandHandler>>();
    }

    // ─── GetAll ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_DelegatesToRepository()
    {
        var paged = new LabourChargeInvoicePagedResponse
        {
            Data = new List<LabourChargeInvoiceMasterDto> { new() { InvoiceCode = 1 } },
            TotalCount = 1
        };
        var parameters = new LabourChargeInvoiceQueryParameters { CompanyCode = 1 };
        _repository.GetAllAsync(parameters).Returns(paged);

        var handler = new GetAllLabourChargeInvoicesQueryHandler(_repository, _getAllLogger);
        var result = await handler.Handle(new GetAllLabourChargeInvoicesQuery { Parameters = parameters }, default);

        result.TotalCount.Should().Be(1);
        result.Data.Should().HaveCount(1);
    }

    // ─── GetById ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_WhenFound_ReturnsDto()
    {
        var invoice = new LabourChargeInvoiceMasterDto { InvoiceCode = 5, CompanyCode = 1 };
        _repository.GetByIdAsync(5, 1).Returns(invoice);

        var handler = new GetLabourChargeInvoiceByIdQueryHandler(_repository, _getByIdLogger);
        var result = await handler.Handle(new GetLabourChargeInvoiceByIdQuery { InvoiceCode = 5, CompanyCode = 1 }, default);

        result.Should().NotBeNull();
        result!.InvoiceCode.Should().Be(5);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNull()
    {
        _repository.GetByIdAsync(999, 1).Returns((LabourChargeInvoiceMasterDto?)null);

        var handler = new GetLabourChargeInvoiceByIdQueryHandler(_repository, _getByIdLogger);
        var result = await handler.Handle(new GetLabourChargeInvoiceByIdQuery { InvoiceCode = 999, CompanyCode = 1 }, default);

        result.Should().BeNull();
    }

    // ─── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_CallsRepositoryAndReturnsResult()
    {
        var returned = new LabourChargeInvoiceMasterDto { InvoiceCode = 10, CompanyCode = 1, InvoiceNumber = 55 };
        _repository.CreateAsync(Arg.Any<CreateLabourChargeInvoiceRequest>()).Returns(returned);

        var handler = new CreateLabourChargeInvoiceCommandHandler(_repository, _createLogger);
        var cmd = new CreateLabourChargeInvoiceCommand
        {
            CompanyCode  = 1,
            CustomerCode = 2,
            InvoiceDate  = DateTime.Today,
            Details      = new()
        };

        var result = await handler.Handle(cmd, default);

        result.InvoiceCode.Should().Be(10);
        await _repository.Received(1).CreateAsync(Arg.Any<CreateLabourChargeInvoiceRequest>());
    }

    // ─── Update ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_CallsRepositoryAndReturnsResult()
    {
        var returned = new LabourChargeInvoiceMasterDto { InvoiceCode = 10, CompanyCode = 1 };
        _repository.UpdateAsync(Arg.Any<UpdateLabourChargeInvoiceRequest>()).Returns(returned);

        var handler = new UpdateLabourChargeInvoiceCommandHandler(_repository, _updateLogger);
        var cmd = new UpdateLabourChargeInvoiceCommand
        {
            InvoiceCode  = 10,
            CompanyCode  = 1,
            CustomerCode = 2,
            InvoiceDate  = DateTime.Today,
            Details      = new()
        };

        var result = await handler.Handle(cmd, default);

        result.InvoiceCode.Should().Be(10);
        await _repository.Received(1).UpdateAsync(Arg.Any<UpdateLabourChargeInvoiceRequest>());
    }

    [Fact]
    public async Task Update_WhenRepositoryReturnsNull_ThrowsInvalidOperation()
    {
        _repository.UpdateAsync(Arg.Any<UpdateLabourChargeInvoiceRequest>()).Returns((LabourChargeInvoiceMasterDto?)null);

        var handler = new UpdateLabourChargeInvoiceCommandHandler(_repository, _updateLogger);
        var cmd = new UpdateLabourChargeInvoiceCommand { InvoiceCode = 999, CompanyCode = 1, InvoiceDate = DateTime.Today, Details = new() };

        var act = async () => await handler.Handle(cmd, default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ─── Delete ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_WhenSuccessful_ReturnsTrue()
    {
        _repository.DeleteAsync(10, 1).Returns(true);
        var handler = new DeleteLabourChargeInvoiceCommandHandler(_repository, _deleteLogger);

        var result = await handler.Handle(new DeleteLabourChargeInvoiceCommand { InvoiceCode = 10, CompanyCode = 1 }, default);

        result.Should().BeTrue();
        await _repository.Received(1).DeleteAsync(10, 1);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ReturnsFalse()
    {
        _repository.DeleteAsync(999, 1).Returns(false);
        var handler = new DeleteLabourChargeInvoiceCommandHandler(_repository, _deleteLogger);

        var result = await handler.Handle(new DeleteLabourChargeInvoiceCommand { InvoiceCode = 999, CompanyCode = 1 }, default);

        result.Should().BeFalse();
    }

    // ─── Lock ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Lock_WhenSuccessful_ReturnsTrue()
    {
        _repository.LockAsync(10, 1, 0).Returns(true);
        var handler = new LockLabourChargeInvoiceCommandHandler(_repository);

        var result = await handler.Handle(new LockLabourChargeInvoiceCommand { InvoiceCode = 10, CompanyCode = 1, LockedByUserId = 0 }, default);

        result.Should().BeTrue();
        await _repository.Received(1).LockAsync(10, 1, 0);
    }

    [Fact]
    public async Task Lock_WhenAlreadyLocked_ReturnsFalse()
    {
        _repository.LockAsync(10, 1, 0).Returns(false);
        var handler = new LockLabourChargeInvoiceCommandHandler(_repository);

        var result = await handler.Handle(new LockLabourChargeInvoiceCommand { InvoiceCode = 10, CompanyCode = 1, LockedByUserId = 0 }, default);

        result.Should().BeFalse();
    }

    // ─── Unlock ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Unlock_WhenSuccessful_ReturnsTrue()
    {
        _repository.UnlockAsync(10, 1).Returns(true);
        var handler = new UnlockLabourChargeInvoiceCommandHandler(_repository, _unlockLogger);

        var result = await handler.Handle(new UnlockLabourChargeInvoiceCommand { InvoiceCode = 10, CompanyCode = 1 }, default);

        result.Should().BeTrue();
        await _repository.Received(1).UnlockAsync(10, 1);
    }

    [Fact]
    public async Task Unlock_WhenNotLocked_ReturnsFalse()
    {
        _repository.UnlockAsync(10, 1).Returns(false);
        var handler = new UnlockLabourChargeInvoiceCommandHandler(_repository, _unlockLogger);

        var result = await handler.Handle(new UnlockLabourChargeInvoiceCommand { InvoiceCode = 10, CompanyCode = 1 }, default);

        result.Should().BeFalse();
    }

    // ─── Print (LCI-05) ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetPrintData_WhenFound_ReturnsPrintDto()
    {
        var printDto = new LabourChargeInvoicePrintDto
        {
            InvoiceCode = 10, InvoiceNumber = 42, CompanyName = "Test Co",
            CustomerName = "Customer A", GrossAmount = 1000m
        };
        _repository.GetPrintDataAsync(10, 1).Returns(printDto);

        var handler = new GetLabourChargeInvoicePrintDataQueryHandler(_repository);
        var result = await handler.Handle(
            new GetLabourChargeInvoicePrintDataQuery { InvoiceCode = 10, CompanyCode = 1 }, default);

        result.Should().NotBeNull();
        result!.InvoiceNumber.Should().Be(42);
        result.GrossAmount.Should().Be(1000m);
    }

    [Fact]
    public async Task GetPrintData_WhenNotFound_ReturnsNull()
    {
        _repository.GetPrintDataAsync(999, 1).Returns((LabourChargeInvoicePrintDto?)null);

        var handler = new GetLabourChargeInvoicePrintDataQueryHandler(_repository);
        var result = await handler.Handle(
            new GetLabourChargeInvoicePrintDataQuery { InvoiceCode = 999, CompanyCode = 1 }, default);

        result.Should().BeNull();
    }
}
