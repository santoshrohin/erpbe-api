using ErpBE.Application.DeliveryChallan.Queries;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.DeliveryChallan.Handlers;

public class GetDeliveryChallanByIdQueryHandler : IRequestHandler<GetDeliveryChallanByIdQuery, DeliveryChallanMasterDto?>
{
    private readonly IDeliveryChallanRepository _repository;

    public GetDeliveryChallanByIdQueryHandler(IDeliveryChallanRepository repository)
    {
        _repository = repository;
    }

    public Task<DeliveryChallanMasterDto?> Handle(GetDeliveryChallanByIdQuery request, CancellationToken cancellationToken)
        => _repository.GetByIdAsync(request.ChallanCode, request.CompanyCode);
}
