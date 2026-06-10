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

    public Task<bool> Handle(LockCustomerPoCommand request, CancellationToken cancellationToken)
        => _repository.LockAsync(request.PoCode, request.LockedByUserId);
}
