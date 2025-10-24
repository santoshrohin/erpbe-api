using ErpBE.Application.CustomerTypeMaster.Queries;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerTypeMaster.Handlers
{
    public class CheckTypeCodeUniqueQueryHandler : IRequestHandler<CheckTypeCodeUniqueQuery, bool>
    {
        private readonly ICustomerTypeMasterRepository _repository;

        public CheckTypeCodeUniqueQueryHandler(ICustomerTypeMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(CheckTypeCodeUniqueQuery request, CancellationToken cancellationToken)
        {
            return await _repository.IsTypeCodeUniqueAsync(request.TypeCode, request.ExcludeId, request.CompanyId, cancellationToken);
        }
    }
}

