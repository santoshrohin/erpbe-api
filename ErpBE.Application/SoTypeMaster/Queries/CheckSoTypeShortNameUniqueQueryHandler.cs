using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Queries
{
    public class CheckSoTypeShortNameUniqueQueryHandler : IRequestHandler<CheckSoTypeShortNameUniqueQuery, bool>
    {
        private readonly ISoTypeMasterRepository _repository;

        public CheckSoTypeShortNameUniqueQueryHandler(ISoTypeMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(CheckSoTypeShortNameUniqueQuery request, CancellationToken cancellationToken)
        {
            return await _repository.IsShortNameUniqueAsync(
                request.ShortName,
                request.CompanyId,
                request.ExcludeId);
        }
    }
}

