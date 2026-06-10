using ErpBE.Application.DeliveryChallan.Commands;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.DeliveryChallan.Handlers
{
    public class LockDeliveryChallanCommandHandler : IRequestHandler<LockDeliveryChallanCommand, bool>
    {
        private readonly IDeliveryChallanRepository _repository;

        public LockDeliveryChallanCommandHandler(IDeliveryChallanRepository repository)
        {
            _repository = repository;
        }

        public Task<bool> Handle(LockDeliveryChallanCommand request, CancellationToken cancellationToken)
            => _repository.LockAsync(request.ChallanCode, request.CompanyCode, request.LockedByUserId);
    }
}
