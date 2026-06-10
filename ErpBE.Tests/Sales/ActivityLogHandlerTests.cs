using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.CustomerMaster.Handlers;
using ErpBE.Application.CustomerMaster.Interfaces;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ErpBE.Tests.Sales;

public class ActivityLogHandlerTests
{
    private readonly ICustomerMasterRepository _repository;
    private readonly IActivityLogService       _activityLog;
    private readonly ICompanyContext           _ctx;

    public ActivityLogHandlerTests()
    {
        _repository  = Substitute.For<ICustomerMasterRepository>();
        _activityLog = Substitute.For<IActivityLogService>();
        _ctx         = Substitute.For<ICompanyContext>();
        _ctx.Username.Returns("admin");
        _ctx.UserCode.Returns(1);
    }

    [Fact]
    public async Task CreateCustomerMaster_WritesInsertLog()
    {
        var returned = new CustomerMasterDto { Id = 5, CompanyId = 1, PartyName = "ACME", Abbreviation = "ACM" };
        _repository.CreateAsync(Arg.Any<CreateCustomerMasterRequest>()).Returns(returned);

        var handler = new CreateCustomerMasterCommandHandler(_repository, _activityLog, _ctx);
        var cmd = new CreateCustomerMasterCommand { CompanyId = 1, PartyName = "ACME", Abbreviation = "ACM" };

        await handler.Handle(cmd, default);

        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "CustomerMaster", @event: "INSERT",
            docName: "Customer Master", docNo: Arg.Any<string>(), docCode: 5,
            userName: "admin", userCode: 1, cancellationToken: default);
    }

    [Fact]
    public async Task DeleteCustomerMaster_WritesDeleteLog()
    {
        var handler = new DeleteCustomerMasterCommandHandler(_repository, _activityLog, _ctx);
        var cmd = new DeleteCustomerMasterCommand { Id = 7, CompanyId = 1 };

        await handler.Handle(cmd, default);

        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "CustomerMaster", @event: "DELETE",
            docName: "Customer Master", docNo: string.Empty, docCode: 7,
            userName: "admin", userCode: 1, cancellationToken: default);
    }

    [Fact]
    public async Task UpdateCustomerMaster_WritesUpdateLog()
    {
        var handler = new UpdateCustomerMasterCommandHandler(_repository, _activityLog, _ctx);
        var cmd = new UpdateCustomerMasterCommand
        {
            Id           = 5,
            CompanyId    = 1,
            PartyName    = "ACME Corp",
            Abbreviation = "ACM"
        };

        await handler.Handle(cmd, default);

        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "CustomerMaster", @event: "UPDATE",
            docName: "Customer Master", docNo: Arg.Any<string>(), docCode: 5,
            userName: "admin", userCode: 1, cancellationToken: default);
    }
}
