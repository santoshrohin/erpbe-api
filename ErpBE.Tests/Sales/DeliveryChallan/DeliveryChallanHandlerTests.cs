using ErpBE.Application.DeliveryChallan.Commands;
using ErpBE.Application.DeliveryChallan.Handlers;
using ErpBE.Application.DeliveryChallan.Queries;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ErpBE.Tests.Sales.DeliveryChallan;

public class DeliveryChallanHandlerTests
{
    private readonly IDeliveryChallanRepository _repository;
    private readonly IActivityLogService        _activityLog;
    private readonly ICompanyContext            _ctx;

    public DeliveryChallanHandlerTests()
    {
        _repository  = Substitute.For<IDeliveryChallanRepository>();
        _activityLog = Substitute.For<IActivityLogService>();
        _ctx         = Substitute.For<ICompanyContext>();
        _ctx.Username.Returns("testuser");
        _ctx.UserCode.Returns(1);
    }

    // ─── GetAll ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_DelegatesToRepository()
    {
        var expected = (new List<DeliveryChallanMasterDto> { new() { ChallanCode = 1 } }.AsEnumerable(), 1);
        var parameters = new DeliveryChallanQueryParameters { CompanyCode = 1 };
        _repository.GetAllAsync(parameters).Returns(expected);

        var handler = new GetAllDeliveryChallansQueryHandler(_repository);
        var (data, count) = await handler.Handle(new GetAllDeliveryChallansQuery { Parameters = parameters }, default);

        count.Should().Be(1);
        data.Should().HaveCount(1);
    }

    // ─── GetById ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_WhenFound_ReturnsMaster()
    {
        var challan = new DeliveryChallanMasterDto { ChallanCode = 5, CompanyCode = 1 };
        _repository.GetByIdAsync(5, 1).Returns(challan);

        var handler = new GetDeliveryChallanByIdQueryHandler(_repository);
        var result = await handler.Handle(new GetDeliveryChallanByIdQuery { ChallanCode = 5, CompanyCode = 1 }, default);

        result.Should().NotBeNull();
        result!.ChallanCode.Should().Be(5);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNull()
    {
        _repository.GetByIdAsync(999, 1).Returns((DeliveryChallanMasterDto?)null);

        var handler = new GetDeliveryChallanByIdQueryHandler(_repository);
        var result = await handler.Handle(new GetDeliveryChallanByIdQuery { ChallanCode = 999, CompanyCode = 1 }, default);

        result.Should().BeNull();
    }

    // ─── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_CallsRepositoryAndWritesLog()
    {
        var returned = new DeliveryChallanMasterDto { ChallanCode = 10, CompanyCode = 1, ChallanNumber = 1 };
        _repository.CreateAsync(Arg.Any<CreateDeliveryChallanRequest>()).Returns(returned);

        var handler = new CreateDeliveryChallanCommandHandler(_repository, _activityLog, _ctx);
        var cmd = new CreateDeliveryChallanCommand { CompanyCode = 1, ChallanDate = DateTime.Today, Details = new() };

        var result = await handler.Handle(cmd, default);

        result.ChallanCode.Should().Be(10);
        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "DeliveryChallan", @event: "INSERT",
            docName: "Delivery Challan", docNo: Arg.Any<string>(), docCode: 10,
            userName: "testuser", userCode: 1, cancellationToken: default);
    }

    // ─── Update ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_CallsRepositoryAndWritesLog()
    {
        var returned = new DeliveryChallanMasterDto { ChallanCode = 10, CompanyCode = 1, ChallanNumber = 1 };
        _repository.UpdateAsync(Arg.Any<UpdateDeliveryChallanRequest>()).Returns(returned);

        var handler = new UpdateDeliveryChallanCommandHandler(_repository, _activityLog, _ctx);
        var cmd = new UpdateDeliveryChallanCommand { ChallanCode = 10, CompanyCode = 1, ChallanDate = DateTime.Today, Details = new() };

        var result = await handler.Handle(cmd, default);

        result.ChallanCode.Should().Be(10);
        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "DeliveryChallan", @event: "UPDATE",
            docName: "Delivery Challan", docNo: Arg.Any<string>(), docCode: 10,
            userName: "testuser", userCode: 1, cancellationToken: default);
    }

    // ─── Delete ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_WhenSuccessful_WritesLog()
    {
        _repository.DeleteAsync(10, 1).Returns(true);
        var handler = new DeleteDeliveryChallanCommandHandler(_repository, _activityLog, _ctx);

        var result = await handler.Handle(new DeleteDeliveryChallanCommand { ChallanCode = 10, CompanyCode = 1 }, default);

        result.Should().BeTrue();
        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "DeliveryChallan", @event: "DELETE",
            docName: "Delivery Challan", docNo: string.Empty, docCode: 10,
            userName: "testuser", userCode: 1, cancellationToken: default);
    }

    [Fact]
    public async Task Delete_WhenNotFound_DoesNotWriteLog()
    {
        _repository.DeleteAsync(999, 1).Returns(false);
        var handler = new DeleteDeliveryChallanCommandHandler(_repository, _activityLog, _ctx);

        var result = await handler.Handle(new DeleteDeliveryChallanCommand { ChallanCode = 999, CompanyCode = 1 }, default);

        result.Should().BeFalse();
        await _activityLog.DidNotReceive().WriteLogAsync(
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(),
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>());
    }

    // ─── Lock ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Lock_WhenSuccessful_ReturnsTrue()
    {
        _repository.LockAsync(10, 1, 0).Returns(true);
        var handler = new LockDeliveryChallanCommandHandler(_repository);

        var result = await handler.Handle(new LockDeliveryChallanCommand { ChallanCode = 10, CompanyCode = 1, LockedByUserId = 0 }, default);

        result.Should().BeTrue();
        await _repository.Received(1).LockAsync(10, 1, 0);
    }

    [Fact]
    public async Task Lock_WhenAlreadyLockedOrNotFound_ReturnsFalse()
    {
        _repository.LockAsync(999, 1, 0).Returns(false);
        var handler = new LockDeliveryChallanCommandHandler(_repository);

        var result = await handler.Handle(new LockDeliveryChallanCommand { ChallanCode = 999, CompanyCode = 1, LockedByUserId = 0 }, default);

        result.Should().BeFalse();
    }

    // ─── Unlock ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Unlock_WhenSuccessful_ReturnsTrue()
    {
        _repository.UnlockAsync(10, 1).Returns(true);
        var logger = Substitute.For<ILogger<UnlockDeliveryChallanCommandHandler>>();
        var handler = new UnlockDeliveryChallanCommandHandler(_repository, logger);

        var result = await handler.Handle(new UnlockDeliveryChallanCommand { ChallanCode = 10, CompanyCode = 1 }, default);

        result.Should().BeTrue();
        await _repository.Received(1).UnlockAsync(10, 1);
    }

    [Fact]
    public async Task Unlock_WhenAlreadyUnlockedOrNotFound_ReturnsFalse()
    {
        _repository.UnlockAsync(999, 1).Returns(false);
        var logger = Substitute.For<ILogger<UnlockDeliveryChallanCommandHandler>>();
        var handler = new UnlockDeliveryChallanCommandHandler(_repository, logger);

        var result = await handler.Handle(new UnlockDeliveryChallanCommand { ChallanCode = 999, CompanyCode = 1 }, default);

        result.Should().BeFalse();
    }

    // ─── Print (DC-06) ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetPrintData_WhenFound_ReturnsPrintDto()
    {
        var printDto = new DeliveryChallanPrintDto
        {
            ChallanCode = 10, ChallanNumber = 5, CompanyName = "Test Co",
            CustomerName = "Customer A"
        };
        _repository.GetPrintDataAsync(10, 1).Returns(printDto);

        var handler = new GetDeliveryChallanPrintDataQueryHandler(_repository);
        var result = await handler.Handle(
            new GetDeliveryChallanPrintDataQuery { ChallanCode = 10, CompanyCode = 1 }, default);

        result.Should().NotBeNull();
        result!.ChallanNumber.Should().Be(5);
        result.CustomerName.Should().Be("Customer A");
    }

    [Fact]
    public async Task GetPrintData_WhenNotFound_ReturnsNull()
    {
        _repository.GetPrintDataAsync(999, 1).Returns((DeliveryChallanPrintDto?)null);

        var handler = new GetDeliveryChallanPrintDataQueryHandler(_repository);
        var result = await handler.Handle(
            new GetDeliveryChallanPrintDataQuery { ChallanCode = 999, CompanyCode = 1 }, default);

        result.Should().BeNull();
    }
}
