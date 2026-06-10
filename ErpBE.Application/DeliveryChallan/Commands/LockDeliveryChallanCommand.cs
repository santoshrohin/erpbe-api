using MediatR;

namespace ErpBE.Application.DeliveryChallan.Commands
{
    public class LockDeliveryChallanCommand : IRequest<bool>
    {
        public int ChallanCode    { get; set; }
        public int CompanyCode    { get; set; }
        public int LockedByUserId { get; set; }
    }
}
