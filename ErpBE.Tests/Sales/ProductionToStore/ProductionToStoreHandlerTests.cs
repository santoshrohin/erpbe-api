using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Application.ProductionToStore.Commands;
using ErpBE.Application.ProductionToStore.Handlers;
using ErpBE.Application.ProductionToStore.Queries;
using ErpBE.Domain.Auth;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ErpBE.Tests.Sales.ProductionToStore;

public class ProductionToStoreHandlerTests
{
    private readonly IProductionToStoreRepository _repository;
    private readonly IActivityLogService          _activityLog;
    private readonly ICompanyContext              _ctx;

    public ProductionToStoreHandlerTests()
    {
        _repository  = Substitute.For<IProductionToStoreRepository>();
        _activityLog = Substitute.For<IActivityLogService>();
        _ctx         = Substitute.For<ICompanyContext>();
        _ctx.Username.Returns("testuser");
        _ctx.UserCode.Returns(1);
    }

    // ─── GetAll ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_DelegatesToRepository()
    {
        var expected = (new List<ProductionToStoreMasterDto> { new() { ProductionCode = 1 } }.AsEnumerable(), 1);
        var parameters = new ProductionToStoreQueryParameters { CompanyCode = 1 };
        _repository.GetAllAsync(parameters).Returns(expected);

        var handler = new GetAllProductionToStoreQueryHandler(_repository);
        var (data, count) = await handler.Handle(new GetAllProductionToStoreQuery { Parameters = parameters }, default);

        count.Should().Be(1);
        data.Should().HaveCount(1);
    }

    // ─── GetById ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_WhenFound_ReturnsDto()
    {
        var record = new ProductionToStoreMasterDto { ProductionCode = 5, CompanyCode = 1 };
        _repository.GetByIdAsync(5, 1).Returns(record);

        var handler = new GetProductionToStoreByIdQueryHandler(_repository);
        var result = await handler.Handle(new GetProductionToStoreByIdQuery { ProductionCode = 5, CompanyCode = 1 }, default);

        result.Should().NotBeNull();
        result!.ProductionCode.Should().Be(5);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNull()
    {
        _repository.GetByIdAsync(999, 1).Returns((ProductionToStoreMasterDto?)null);

        var handler = new GetProductionToStoreByIdQueryHandler(_repository);
        var result = await handler.Handle(new GetProductionToStoreByIdQuery { ProductionCode = 999, CompanyCode = 1 }, default);

        result.Should().BeNull();
    }

    // ─── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_CallsRepositoryAndWritesLog()
    {
        var returned = new ProductionToStoreMasterDto { ProductionCode = 10, CompanyCode = 1, GinNumber = 77 };
        _repository.CreateAsync(Arg.Any<CreateProductionToStoreRequest>()).Returns(returned);

        var handler = new CreateProductionToStoreCommandHandler(_repository, _activityLog, _ctx);
        var cmd = new CreateProductionToStoreCommand { CompanyCode = 1, GinDate = DateTime.Today, Details = new() };

        var result = await handler.Handle(cmd, default);

        result.ProductionCode.Should().Be(10);
        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "ProductionToStore", @event: "INSERT",
            docName: "Production To Store", docNo: "77", docCode: 10,
            userName: "testuser", userCode: 1, cancellationToken: default);
    }

    // ─── Update ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_CallsRepositoryAndWritesLog()
    {
        var returned = new ProductionToStoreMasterDto { ProductionCode = 10, CompanyCode = 1, GinNumber = 77 };
        _repository.UpdateAsync(Arg.Any<UpdateProductionToStoreRequest>()).Returns(returned);

        var handler = new UpdateProductionToStoreCommandHandler(_repository, _activityLog, _ctx);
        var cmd = new UpdateProductionToStoreCommand { ProductionCode = 10, CompanyCode = 1, GinDate = DateTime.Today, Details = new() };

        var result = await handler.Handle(cmd, default);

        result.ProductionCode.Should().Be(10);
        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "ProductionToStore", @event: "UPDATE",
            docName: "Production To Store", docNo: "77", docCode: 10,
            userName: "testuser", userCode: 1, cancellationToken: default);
    }

    // ─── Delete ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_WhenSuccessful_WritesLog()
    {
        _repository.DeleteAsync(10, 1).Returns(true);
        var handler = new DeleteProductionToStoreCommandHandler(_repository, _activityLog, _ctx);

        var result = await handler.Handle(new DeleteProductionToStoreCommand { ProductionCode = 10, CompanyCode = 1 }, default);

        result.Should().BeTrue();
        await _activityLog.Received(1).WriteLogAsync(
            companyId: 1, source: "ProductionToStore", @event: "DELETE",
            docName: "Production To Store", docNo: string.Empty, docCode: 10,
            userName: "testuser", userCode: 1, cancellationToken: default);
    }

    [Fact]
    public async Task Delete_WhenNotFound_DoesNotWriteLog()
    {
        _repository.DeleteAsync(999, 1).Returns(false);
        var handler = new DeleteProductionToStoreCommandHandler(_repository, _activityLog, _ctx);

        var result = await handler.Handle(new DeleteProductionToStoreCommand { ProductionCode = 999, CompanyCode = 1 }, default);

        result.Should().BeFalse();
        await _activityLog.DidNotReceive().WriteLogAsync(
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(),
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>());
    }
}
