using ErpBE.Application.Interfaces;
using ErpBE.Application.TaxInvoice.Commands;
using ErpBE.Application.TaxInvoice.Handlers;
using ErpBE.Domain.Auth;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ErpBE.Tests.Sales;

public class ApproveTaxInvoiceHandlerTests
{
    private readonly ITaxInvoiceRepository _repository;
    private readonly IActivityLogService   _activityLog;
    private readonly ICompanyContext       _ctx;

    public ApproveTaxInvoiceHandlerTests()
    {
        _repository  = Substitute.For<ITaxInvoiceRepository>();
        _activityLog = Substitute.For<IActivityLogService>();
        _ctx         = Substitute.For<ICompanyContext>();
        _ctx.Username.Returns("testuser");
        _ctx.UserCode.Returns(1);
    }

    [Fact]
    public async Task Approve_WhenInvoiceExists_ReturnsTrue_AndWritesLog()
    {
        _repository.ApproveAsync(99, 1).Returns(true);
        var handler = new ApproveTaxInvoiceCommandHandler(_repository, _activityLog, _ctx);

        var result = await handler.Handle(new ApproveTaxInvoiceCommand { InvoiceCode = 99, CompanyCode = 1 }, default);

        result.Should().BeTrue();
        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1,
            source:    "TaxInvoice",
            @event:    "APPROVE",
            docName:   "Tax Invoice",
            docNo:     string.Empty,
            docCode:   99,
            userName:  "testuser",
            userCode:  1,
            cancellationToken: default);
    }

    [Fact]
    public async Task Approve_WhenInvoiceNotFound_ReturnsFalse_AndDoesNotLog()
    {
        _repository.ApproveAsync(99, 1).Returns(false);
        var handler = new ApproveTaxInvoiceCommandHandler(_repository, _activityLog, _ctx);

        var result = await handler.Handle(new ApproveTaxInvoiceCommand { InvoiceCode = 99, CompanyCode = 1 }, default);

        result.Should().BeFalse();
        await _activityLog.DidNotReceive().WriteLogAsync(
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(),
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>());
    }
}
