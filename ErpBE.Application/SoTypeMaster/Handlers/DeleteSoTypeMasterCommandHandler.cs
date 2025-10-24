using ErpBE.Application.Interfaces;
using ErpBE.Application.SoTypeMaster.Commands;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Handlers
{
    public class DeleteSoTypeMasterCommandHandler : IRequestHandler<DeleteSoTypeMasterCommand, Unit>
    {
        private readonly ISoTypeMasterRepository _repository;

        public DeleteSoTypeMasterCommandHandler(ISoTypeMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(DeleteSoTypeMasterCommand request, CancellationToken cancellationToken)
        {
            // Check if record exists
            var existing = await _repository.GetByIdAsync(request.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"SO Type Master with ID '{request.Id}' not found.");
            }

            // Check if fixed record (legacy business rule)
            var isFixed = await _repository.IsFixedRecordAsync(request.Id);
            if (isFixed)
            {
                throw new InvalidOperationException("These record is fixed");
            }

            // Check if used in Customer PO (legacy business rule)
            var isUsed = await _repository.IsUsedInCustomerPOAsync(request.Id);
            if (isUsed)
            {
                throw new InvalidOperationException("You cant delete this record it has used in Sales Order");
            }

            var success = await _repository.DeleteAsync(request.Id);
            if (!success)
            {
                throw new InvalidOperationException("Failed to delete SO Type Master.");
            }

            return Unit.Value;
        }
    }
}

