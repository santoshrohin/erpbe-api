using ErpBE.Application.TaxInvoice.Commands;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.TaxInvoice.Handlers
{
    public class DeleteTaxInvoiceCommandHandler : IRequestHandler<DeleteTaxInvoiceCommand, bool>
    {
        private readonly ITaxInvoiceRepository                 _repository;
        private readonly IActivityLogService                   _activityLog;
        private readonly ICompanyContext                       _ctx;
        private readonly ILogger<DeleteTaxInvoiceCommandHandler> _logger;

        public DeleteTaxInvoiceCommandHandler(
            ITaxInvoiceRepository                 repository,
            IActivityLogService                   activityLog,
            ICompanyContext                       ctx,
            ILogger<DeleteTaxInvoiceCommandHandler> logger)
        {
            _repository  = repository;
            _activityLog = activityLog;
            _ctx         = ctx;
            _logger      = logger;
        }

        public async Task<bool> Handle(DeleteTaxInvoiceCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting Tax Invoice: {InvoiceCode} for Company: {CompanyCode}",
                request.InvoiceCode, request.CompanyCode);

            try
            {
                var existingInvoice = await _repository.GetTaxInvoiceByIdAsync(request.InvoiceCode, request.CompanyCode);
                if (existingInvoice == null)
                    return false;

                var isLocked = await _repository.IsInvoiceLockedAsync(request.InvoiceCode);
                if (isLocked)
                    throw new InvalidOperationException("This invoice is currently being modified by another user. Cannot delete.");

                var result = await _repository.DeleteTaxInvoiceAsync(request.InvoiceCode, request.CompanyCode);

                if (result)
                {
                    _logger.LogInformation("Tax Invoice deleted. Code: {InvoiceCode}", request.InvoiceCode);
                    await _activityLog.WriteLogAsync(
                        companyId: request.CompanyCode,
                        source:    "TaxInvoice",
                        @event:    "DELETE",
                        docName:   "Tax Invoice",
                        docNo:     string.Empty,
                        docCode:   request.InvoiceCode,
                        userName:  _ctx.Username,
                        userCode:  _ctx.UserCode,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Failed to delete Tax Invoice. Code: {InvoiceCode}", request.InvoiceCode);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Tax Invoice: {InvoiceCode}", request.InvoiceCode);
                throw;
            }
        }
    }
}

