using ErpBE.Application.Interfaces;
using ErpBE.Application.SoTypeMaster.Commands;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Handlers
{
    public class CreateSoTypeMasterCommandHandler : IRequestHandler<CreateSoTypeMasterCommand, int>
    {
        private readonly ISoTypeMasterRepository _repository;

        public CreateSoTypeMasterCommandHandler(ISoTypeMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateSoTypeMasterCommand request, CancellationToken cancellationToken)
        {
            // Trim all inputs as per legacy logic
            request.Request.ShortName = request.Request.ShortName.Trim();
            request.Request.Description = request.Request.Description.Trim();
            request.Request.FirstLetter = request.Request.FirstLetter.Trim();

            // Check uniqueness (case-insensitive) as per legacy logic
            var isUnique = await _repository.IsShortNameUniqueAsync(
                request.Request.ShortName,
                request.Request.CompanyId);

            if (!isUnique)
            {
                throw new InvalidOperationException("Short Name Already Exists");
            }

            return await _repository.CreateAsync(request.Request);
        }
    }
}

