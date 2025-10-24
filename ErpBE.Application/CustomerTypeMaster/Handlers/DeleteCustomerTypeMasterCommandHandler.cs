using ErpBE.Application.Audit;
using ErpBE.Application.CustomerTypeMaster.Commands;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerTypeMaster.Handlers
{
    public class DeleteCustomerTypeMasterCommandHandler : IRequestHandler<DeleteCustomerTypeMasterCommand, Unit>
    {
        private readonly ICustomerTypeMasterRepository _repository;
        private readonly IAuditService _auditService;

        public DeleteCustomerTypeMasterCommandHandler(
            ICustomerTypeMasterRepository repository,
            IAuditService auditService)
        {
            _repository = repository;
            _auditService = auditService;
        }

        public async Task<Unit> Handle(DeleteCustomerTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var existingRecord = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (existingRecord == null)
            {
                throw new KeyNotFoundException($"Customer Type Master with ID '{request.Id}' not found.");
            }

            if (await _repository.IsModifiedByAnotherUserAsync(request.Id, cancellationToken))
            {
                throw new InvalidOperationException("This record is currently being modified by another user. Please try again later.");
            }

            if (await _repository.IsUsedInPartyMasterAsync(request.Id, cancellationToken))
            {
                throw new InvalidOperationException("You cannot delete this record as it is being used in Customer Master.");
            }

            var success = await _repository.DeleteAsync(request.Id, request.CompanyId, cancellationToken);

            if (!success)
            {
                throw new InvalidOperationException("Failed to delete Customer Type Master.");
            }

            await _auditService.LogDeleteAsync(
                tableName: "CUSTOMER_TYPE_MASTER",
                recordId: request.Id,
                oldValues: existingRecord,
                deletedBy: "System"
            );

            return Unit.Value;
        }
    }
}

