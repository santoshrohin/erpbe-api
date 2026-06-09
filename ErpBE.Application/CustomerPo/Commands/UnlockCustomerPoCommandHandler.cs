using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerPo.Commands;

public class UnlockCustomerPoCommandHandler : IRequestHandler<UnlockCustomerPoCommand, bool>
{
    private readonly ICustomerPoRepository _repository;

    public UnlockCustomerPoCommandHandler(ICustomerPoRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UnlockCustomerPoCommand request, CancellationToken cancellationToken)
    {
        await _repository.UnlockAsync(request.PoCode);
        return true;
    }
}
