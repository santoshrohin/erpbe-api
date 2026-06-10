using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.DeliveryChallan.Queries;

public class GetDeliveryChallanByIdQuery : IRequest<DeliveryChallanMasterDto?>
{
    public int ChallanCode { get; set; }
    public int CompanyCode { get; set; }
}
