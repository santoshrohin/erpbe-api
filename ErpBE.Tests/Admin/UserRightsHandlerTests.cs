using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Application.UserRights.Commands;
using ErpBE.Application.UserRights.Handlers;
using ErpBE.Application.UserRights.Queries;
using ErpBE.Domain.Auth;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ErpBE.Tests.Admin;

public class UserRightsHandlerTests
{
    private readonly IUserRightRepository _repository;
    private readonly IActivityLogService  _activityLog;
    private readonly ICompanyContext      _ctx;

    public UserRightsHandlerTests()
    {
        _repository  = Substitute.For<IUserRightRepository>();
        _activityLog = Substitute.For<IActivityLogService>();
        _ctx         = Substitute.For<ICompanyContext>();
        _ctx.Username.Returns("admin");
        _ctx.UserCode.Returns(1);
        _ctx.CompanyId.Returns(1);
    }

    // ─── GetScreenMasters ────────────────────────────────────────────────────

    [Fact]
    public async Task GetScreenMasters_DelegatesToRepository()
    {
        var screens = new List<ScreenMasterDto>
        {
            new() { ScreenCode = 75, ScreenName = "Sales", ModuleCode = 75 },
            new() { ScreenCode = 77, ScreenName = "Admin", ModuleCode = 77 }
        };
        _repository.GetScreensAsync(default).Returns(screens);

        var handler = new GetScreenMastersQueryHandler(_repository);
        var result  = await handler.Handle(new GetScreenMastersQuery(), default);

        result.Should().HaveCount(2);
    }

    // ─── GetUserRights ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserRights_DelegatesToRepository()
    {
        var rights = new List<UserRightDto>
        {
            new() { UserCode = 5, ScreenCode = 75, Bitmask = "1111000" }
        };
        _repository.GetUserRightsAsync(5, default).Returns(rights);

        var handler = new GetUserRightsQueryHandler(_repository);
        var result  = await handler.Handle(new GetUserRightsQuery { UserCode = 5 }, default);

        result.Should().HaveCount(1);
        result.First().Bitmask.Should().Be("1111000");
    }

    // ─── SaveUserRights ──────────────────────────────────────────────────────

    [Fact]
    public async Task SaveUserRights_WhenSuccessful_WritesLog()
    {
        var rights = new List<UserRightRequest>
        {
            new() { ScreenCode = 75, Bitmask = "1111000" }
        };
        _repository.UpsertUserRightsAsync(5, rights, default).Returns(true);

        var handler = new SaveUserRightsCommandHandler(_repository, _activityLog, _ctx);
        var result  = await handler.Handle(new SaveUserRightsCommand { UserCode = 5, Rights = rights }, default);

        result.Should().BeTrue();
        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "UserRights", @event: "SAVE",
            docName: "User Rights", docNo: string.Empty, docCode: 5,
            userName: "admin", userCode: 1, cancellationToken: default);
    }

    [Fact]
    public async Task SaveUserRights_WhenFails_DoesNotWriteLog()
    {
        _repository.UpsertUserRightsAsync(Arg.Any<int>(), Arg.Any<IEnumerable<UserRightRequest>>(), default).Returns(false);

        var handler = new SaveUserRightsCommandHandler(_repository, _activityLog, _ctx);
        var result  = await handler.Handle(new SaveUserRightsCommand { UserCode = 5, Rights = new() }, default);

        result.Should().BeFalse();
        await _activityLog.DidNotReceive().WriteLogAsync(
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(),
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>());
    }

    // ─── CopyUserRights ──────────────────────────────────────────────────────

    [Fact]
    public async Task CopyUserRights_WhenSuccessful_WritesLog()
    {
        _repository.CopyRightsAsync(3, 5, default).Returns(true);

        var handler = new CopyUserRightsCommandHandler(_repository, _activityLog, _ctx);
        var result  = await handler.Handle(new CopyUserRightsCommand { FromUserCode = 3, ToUserCode = 5 }, default);

        result.Should().BeTrue();
        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "UserRights", @event: "COPY",
            docName: "User Rights", docNo: "from:3", docCode: 5,
            userName: "admin", userCode: 1, cancellationToken: default);
    }

    // ─── DeleteUserRights ────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteUserRights_WhenSuccessful_WritesLog()
    {
        _repository.DeleteUserRightsAsync(5, default).Returns(true);

        var handler = new DeleteUserRightsCommandHandler(_repository, _activityLog, _ctx);
        var result  = await handler.Handle(new DeleteUserRightsCommand { UserCode = 5 }, default);

        result.Should().BeTrue();
        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "UserRights", @event: "DELETE",
            docName: "User Rights", docNo: string.Empty, docCode: 5,
            userName: "admin", userCode: 1, cancellationToken: default);
    }
}
