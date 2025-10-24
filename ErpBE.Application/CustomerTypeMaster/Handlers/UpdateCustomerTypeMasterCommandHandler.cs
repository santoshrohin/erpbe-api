using ErpBE.Application.Audit;
using ErpBE.Application.CustomerTypeMaster.Commands;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerTypeMaster.Handlers
{
    public class UpdateCustomerTypeMasterCommandHandler : IRequestHandler<UpdateCustomerTypeMasterCommand, Unit>
    {
        private readonly ICustomerTypeMasterRepository _repository;
        private readonly IAuditService _auditService;

        public UpdateCustomerTypeMasterCommandHandler(
            ICustomerTypeMasterRepository repository,
            IAuditService auditService)
        {
            _repository = repository;
            _auditService = auditService;
        }

        public async Task<Unit> Handle(UpdateCustomerTypeMasterCommand request, CancellationToken cancellationToken)
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

            if (!await _repository.IsTypeCodeUniqueAsync(request.TypeCode, request.Id, request.CompanyId, cancellationToken))
            {
                throw new InvalidOperationException("Type Code already exists in the system.");
            }

            var updateRequest = new UpdateCustomerTypeMasterRequest
            {
                Id = request.Id,
                CompanyId = request.CompanyId,
                TypeCode = request.TypeCode.Trim(),
                TypeDescription = request.TypeDescription.Trim(),
                FirstLetter = request.FirstLetter.Trim()
            };

            var success = await _repository.UpdateAsync(updateRequest, cancellationToken);

            if (!success)
            {
                throw new InvalidOperationException("Failed to update Customer Type Master.");
            }

            await _auditService.LogUpdateAsync(
                tableName: "CUSTOMER_TYPE_MASTER",
                recordId: request.Id,
                oldValues: existingRecord,
                newValues: updateRequest,
                modifiedBy: "System"
            );

            return Unit.Value;
        }
    }
}

