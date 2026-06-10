using ErpBE.Application.Interfaces;
using ErpBE.Application.LabourChargeInvoice.Commands;
using MediatR;

namespace ErpBE.Application.LabourChargeInvoice.Handlers
{
    public class LockLabourChargeInvoiceCommandHandler
        : IRequestHandler<LockLabourChargeInvoiceCommand, bool>
    {
        private readonly ILabourChargeInvoiceRepository _repository;

        public LockLabourChargeInvoiceCommandHandler(ILabourChargeInvoiceRepository repository)
        {
            _repository = repository;
        }

        public Task<bool> Handle(LockLabourChargeInvoiceCommand request, CancellationToken cancellationToken)
            => _repository.LockAsync(request.InvoiceCode, request.CompanyCode, request.LockedByUserId);
    }
}
