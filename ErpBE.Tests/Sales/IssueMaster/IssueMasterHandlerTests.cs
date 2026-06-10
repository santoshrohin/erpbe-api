using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Application.IssueMaster.Commands;
using ErpBE.Application.IssueMaster.Handlers;
using ErpBE.Application.IssueMaster.Queries;
using ErpBE.Domain.Auth;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ErpBE.Tests.Sales.IssueMaster;

public class IssueMasterHandlerTests
{
    private readonly IIssueMasterRepository _repository;
    private readonly IActivityLogService    _activityLog;
    private readonly ICompanyContext        _ctx;

    public IssueMasterHandlerTests()
    {
        _repository  = Substitute.For<IIssueMasterRepository>();
        _activityLog = Substitute.For<IActivityLogService>();
        _ctx         = Substitute.For<ICompanyContext>();
        _ctx.Username.Returns("testuser");
        _ctx.UserCode.Returns(1);
    }

    // ─── GetAll ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_DelegatesToRepository()
    {
        var expected = (new List<IssueMasterDto> { new() { IssueCode = 1 } }.AsEnumerable(), 1);
        var parameters = new IssueMasterQueryParameters { CompanyCode = 1 };
        _repository.GetAllAsync(parameters).Returns(expected);

        var handler = new GetAllIssueMasterQueryHandler(_repository);
        var (data, count) = await handler.Handle(new GetAllIssueMasterQuery { Parameters = parameters }, default);

        count.Should().Be(1);
        data.Should().HaveCount(1);
    }

    // ─── GetById ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_WhenFound_ReturnsDto()
    {
        var issue = new IssueMasterDto { IssueCode = 5, CompanyCode = 1 };
        _repository.GetByIdAsync(5, 1).Returns(issue);

        var handler = new GetIssueMasterByIdQueryHandler(_repository);
        var result = await handler.Handle(new GetIssueMasterByIdQuery { IssueCode = 5, CompanyCode = 1 }, default);

        result.Should().NotBeNull();
        result!.IssueCode.Should().Be(5);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNull()
    {
        _repository.GetByIdAsync(999, 1).Returns((IssueMasterDto?)null);

        var handler = new GetIssueMasterByIdQueryHandler(_repository);
        var result = await handler.Handle(new GetIssueMasterByIdQuery { IssueCode = 999, CompanyCode = 1 }, default);

        result.Should().BeNull();
    }

    // ─── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_CallsRepositoryAndWritesLog()
    {
        var returned = new IssueMasterDto { IssueCode = 10, CompanyCode = 1, IssueNumber = 42 };
        _repository.CreateAsync(Arg.Any<CreateIssueMasterRequest>()).Returns(returned);

        var handler = new CreateIssueMasterCommandHandler(_repository, _activityLog, _ctx);
        var cmd = new CreateIssueMasterCommand { CompanyCode = 1, IssueDate = DateTime.Today, Details = new() };

        var result = await handler.Handle(cmd, default);

        result.IssueCode.Should().Be(10);
        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "IssueMaster", @event: "INSERT",
            docName: "Issue to Production", docNo: "42", docCode: 10,
            userName: "testuser", userCode: 1, cancellationToken: default);
    }

    // ─── Update ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_CallsRepositoryAndWritesLog()
    {
        var returned = new IssueMasterDto { IssueCode = 10, CompanyCode = 1, IssueNumber = 42 };
        _repository.UpdateAsync(Arg.Any<UpdateIssueMasterRequest>()).Returns(returned);

        var handler = new UpdateIssueMasterCommandHandler(_repository, _activityLog, _ctx);
        var cmd = new UpdateIssueMasterCommand { IssueCode = 10, CompanyCode = 1, IssueDate = DateTime.Today, Details = new() };

        var result = await handler.Handle(cmd, default);

        result.IssueCode.Should().Be(10);
        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "IssueMaster", @event: "UPDATE",
            docName: "Issue to Production", docNo: "42", docCode: 10,
            userName: "testuser", userCode: 1, cancellationToken: default);
    }

    // ─── Delete ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_WhenSuccessful_WritesLog()
    {
        _repository.DeleteAsync(10, 1).Returns(true);
        var handler = new DeleteIssueMasterCommandHandler(_repository, _activityLog, _ctx);

        var result = await handler.Handle(new DeleteIssueMasterCommand { IssueCode = 10, CompanyCode = 1 }, default);

        result.Should().BeTrue();
        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "IssueMaster", @event: "DELETE",
            docName: "Issue to Production", docNo: string.Empty, docCode: 10,
            userName: "testuser", userCode: 1, cancellationToken: default);
    }

    [Fact]
    public async Task Delete_WhenNotFound_DoesNotWriteLog()
    {
        _repository.DeleteAsync(999, 1).Returns(false);
        var handler = new DeleteIssueMasterCommandHandler(_repository, _activityLog, _ctx);

        var result = await handler.Handle(new DeleteIssueMasterCommand { IssueCode = 999, CompanyCode = 1 }, default);

        result.Should().BeFalse();
        await _activityLog.DidNotReceive().WriteLogAsync(
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(),
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>());
    }
}
