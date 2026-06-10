using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.DeliveryChallan.Queries;

public class GetAllDeliveryChallansQuery : IRequest<(IEnumerable<DeliveryChallanMasterDto> Data, int TotalCount)>
{
    public DeliveryChallanQueryParameters Parameters { get; set; } = new();
}
