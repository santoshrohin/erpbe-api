using ErpBE.Application.Interfaces;
using ErpBE.Application.SoTypeMaster.Commands;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Handlers
{
    public class UpdateSoTypeMasterCommandHandler : IRequestHandler<UpdateSoTypeMasterCommand, Unit>
    {
        private readonly ISoTypeMasterRepository _repository;

        public UpdateSoTypeMasterCommandHandler(ISoTypeMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(UpdateSoTypeMasterCommand request, CancellationToken cancellationToken)
        {
            // Check if record exists
            var existing = await _repository.GetByIdAsync(request.Request.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"SO Type Master with ID '{request.Request.Id}' not found.");
            }

            // Trim all inputs as per legacy logic
            request.Request.ShortName = request.Request.ShortName.Trim();
            request.Request.Description = request.Request.Description.Trim();
            request.Request.FirstLetter = request.Request.FirstLetter.Trim();

            // Check uniqueness excluding current ID (case-insensitive)
            var isUnique = await _repository.IsShortNameUniqueAsync(
                request.Request.ShortName,
                request.Request.CompanyId,
                request.Request.Id);

            if (!isUnique)
            {
                throw new InvalidOperationException("Short Name Already Exists");
            }

            var success = await _repository.UpdateAsync(request.Request);
            if (!success)
            {
                throw new InvalidOperationException("Failed to update SO Type Master.");
            }

            return Unit.Value;
        }
    }
}

