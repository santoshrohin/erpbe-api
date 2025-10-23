using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.UnitMaster.Queries
{
    public class CheckUnitNameUniqueQueryHandler : IRequestHandler<CheckUnitNameUniqueQuery, bool>
    {
        private readonly IUnitMasterRepository _repository;

        public CheckUnitNameUniqueQueryHandler(IUnitMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(CheckUnitNameUniqueQuery request, CancellationToken cancellationToken)
        {
            return await _repository.IsUnitNameUniqueAsync(request.UnitName, request.CompanyId, request.ExcludeId);
        }
    }
}

