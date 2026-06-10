using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.DeliveryChallan.Queries;

public class GetDeliveryChallanPrintDataQueryHandler
    : IRequestHandler<GetDeliveryChallanPrintDataQuery, DeliveryChallanPrintDto?>
{
    private readonly IDeliveryChallanRepository _repository;

    public GetDeliveryChallanPrintDataQueryHandler(IDeliveryChallanRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeliveryChallanPrintDto?> Handle(
        GetDeliveryChallanPrintDataQuery request,
        CancellationToken cancellationToken)
    {
        return await _repository.GetPrintDataAsync(request.ChallanCode, request.CompanyCode);
    }
}
