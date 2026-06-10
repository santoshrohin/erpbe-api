using MediatR;

namespace ErpBE.Application.DeliveryChallan.Commands;

public class DeleteDeliveryChallanCommand : IRequest<bool>
{
    public int ChallanCode { get; set; }
    public int CompanyCode { get; set; }
}
