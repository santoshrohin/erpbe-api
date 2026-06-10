using ErpBE.Application.TaxInvoice.Commands;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.TaxInvoice.Handlers
{
    public class LockTaxInvoiceCommandHandler : IRequestHandler<LockTaxInvoiceCommand, bool>
    {
        private readonly ITaxInvoiceRepository _repository;
        public LockTaxInvoiceCommandHandler(ITaxInvoiceRepository repository) => _repository = repository;
        public Task<bool> Handle(LockTaxInvoiceCommand request, CancellationToken cancellationToken)
            => _repository.LockInvoiceAsync(request.InvoiceCode, request.LockedByUserId);
    }

    public class UnlockTaxInvoiceCommandHandler : IRequestHandler<UnlockTaxInvoiceCommand, bool>
    {
        private readonly ITaxInvoiceRepository _repository;
        public UnlockTaxInvoiceCommandHandler(ITaxInvoiceRepository repository) => _repository = repository;
        public Task<bool> Handle(UnlockTaxInvoiceCommand request, CancellationToken cancellationToken)
            => _repository.UnlockInvoiceAsync(request.InvoiceCode);
    }
}
