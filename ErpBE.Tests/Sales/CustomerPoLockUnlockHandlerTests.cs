using ErpBE.Application.CustomerPo.Commands;
using ErpBE.Application.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ErpBE.Tests.Sales;

public class CustomerPoLockUnlockHandlerTests
{
    private readonly ICustomerPoRepository _repository;

    public CustomerPoLockUnlockHandlerTests()
    {
        _repository = Substitute.For<ICustomerPoRepository>();
    }

    // ─── Lock ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Lock_WhenAlreadyLocked_ReturnsFalseWithoutCallingLock()
    {
        _repository.IsLockedAsync(42).Returns(true);
        var handler = new LockCustomerPoCommandHandler(_repository);

        var result = await handler.Handle(new LockCustomerPoCommand { PoCode = 42, CompanyId = 1 }, default);

        result.Should().BeFalse();
        await _repository.DidNotReceive().LockAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Lock_WhenNotLocked_CallsLockAndReturnsTrue()
    {
        _repository.IsLockedAsync(42).Returns(false);
        var handler = new LockCustomerPoCommandHandler(_repository);

        var result = await handler.Handle(new LockCustomerPoCommand { PoCode = 42, CompanyId = 1 }, default);

        result.Should().BeTrue();
        await _repository.Received(1).LockAsync(42);
    }

    // ─── Unlock ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Unlock_AlwaysCallsUnlockAndReturnsTrue()
    {
        var handler = new UnlockCustomerPoCommandHandler(_repository);

        var result = await handler.Handle(new UnlockCustomerPoCommand { PoCode = 42, CompanyId = 1 }, default);

        result.Should().BeTrue();
        await _repository.Received(1).UnlockAsync(42);
    }
}
