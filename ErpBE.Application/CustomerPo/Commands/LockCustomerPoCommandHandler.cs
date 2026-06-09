using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerPo.Commands;

public class LockCustomerPoCommandHandler : IRequestHandler<LockCustomerPoCommand, bool>
{
    private readonly ICustomerPoRepository _repository;

    public LockCustomerPoCommandHandler(ICustomerPoRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(LockCustomerPoCommand request, CancellationToken cancellationToken)
    {
        var isAlreadyLocked = await _repository.IsLockedAsync(request.PoCode);
        if (isAlreadyLocked)
            return false;

        await _repository.LockAsync(request.PoCode);
        return true;
    }
}
