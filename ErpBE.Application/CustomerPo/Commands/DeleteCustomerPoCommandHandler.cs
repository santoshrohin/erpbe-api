using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerPo.Commands;

/// <summary>
/// Handler for deleting a Customer PO
/// </summary>
public class DeleteCustomerPoCommandHandler : IRequestHandler<DeleteCustomerPoCommand, bool>
{
    private readonly ICustomerPoRepository _repository;

    public DeleteCustomerPoCommandHandler(ICustomerPoRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteCustomerPoCommand request, CancellationToken cancellationToken)
    {
        return await _repository.DeleteAsync(request.PoCode, request.CompanyId);
    }
}

