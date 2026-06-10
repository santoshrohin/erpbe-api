using ErpBE.Application.DeliveryChallan.Queries;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.DeliveryChallan.Handlers;

public class GetAllDeliveryChallansQueryHandler
    : IRequestHandler<GetAllDeliveryChallansQuery, (IEnumerable<DeliveryChallanMasterDto> Data, int TotalCount)>
{
    private readonly IDeliveryChallanRepository _repository;

    public GetAllDeliveryChallansQueryHandler(IDeliveryChallanRepository repository)
    {
        _repository = repository;
    }

    public Task<(IEnumerable<DeliveryChallanMasterDto> Data, int TotalCount)> Handle(
        GetAllDeliveryChallansQuery request, CancellationToken cancellationToken)
        => _repository.GetAllAsync(request.Parameters);
}
