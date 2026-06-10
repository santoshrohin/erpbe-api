using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.DeliveryChallan.Queries;

public class GetDeliveryChallanPrintDataQuery : IRequest<DeliveryChallanPrintDto?>
{
    public int ChallanCode { get; set; }
    public int CompanyCode { get; set; }
}
