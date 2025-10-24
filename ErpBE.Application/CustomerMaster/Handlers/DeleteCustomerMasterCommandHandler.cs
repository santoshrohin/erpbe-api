using ErpBE.Application.Audit;
using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerMaster.Handlers
{
    public class DeleteCustomerMasterCommandHandler : IRequestHandler<DeleteCustomerMasterCommand, Unit>
    {
        private readonly ICustomerMasterRepository _repository;
        private readonly IAuditService _auditService;

        public DeleteCustomerMasterCommandHandler(ICustomerMasterRepository repository, IAuditService auditService)
        {
            _repository = repository;
            _auditService = auditService;
        }

        public async Task<Unit> Handle(DeleteCustomerMasterCommand request, CancellationToken cancellationToken)
        {
            var existingRecord = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (existingRecord == null)
            {
                throw new KeyNotFoundException($"Customer Master with ID '{request.Id}' not found.");
            }

            if (await _repository.IsModifiedByAnotherUserAsync(request.Id, cancellationToken))
            {
                throw new InvalidOperationException("This record is currently being modified by another user. Please try again later.");
            }

            if (await _repository.IsUsedInTransactionsAsync(request.Id, cancellationToken))
            {
                throw new InvalidOperationException("You cannot delete this record as it is being used in transactions.");
            }

            var success = await _repository.DeleteAsync(request.Id, request.CompanyId, cancellationToken);

            if (!success)
            {
                throw new InvalidOperationException("Failed to delete Customer Master.");
            }

            await _auditService.LogDeleteAsync(
                tableName: "PARTY_MASTER",
                recordId: request.Id,
                oldValues: existingRecord,
                deletedBy: "System"
            );

            return Unit.Value;
        }
    }
}

