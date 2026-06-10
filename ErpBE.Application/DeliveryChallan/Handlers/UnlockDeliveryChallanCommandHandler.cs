using ErpBE.Application.DeliveryChallan.Commands;
using ErpBE.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.DeliveryChallan.Handlers
{
    public class UnlockDeliveryChallanCommandHandler : IRequestHandler<UnlockDeliveryChallanCommand, bool>
    {
        private readonly IDeliveryChallanRepository _repository;
        private readonly ILogger<UnlockDeliveryChallanCommandHandler> _logger;

        public UnlockDeliveryChallanCommandHandler(
            IDeliveryChallanRepository repository,
            ILogger<UnlockDeliveryChallanCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> Handle(UnlockDeliveryChallanCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Unlocking Delivery Challan: {ChallanCode} for Company: {CompanyCode}",
                request.ChallanCode, request.CompanyCode);

            return await _repository.UnlockAsync(request.ChallanCode, request.CompanyCode);
        }
    }
}
