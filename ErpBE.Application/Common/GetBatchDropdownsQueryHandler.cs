using ErpBE.Application.Common.Models;
using ErpBE.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ErpBE.Application.Common
{
    public class GetBatchDropdownsQueryHandler : IRequestHandler<GetBatchDropdownsQuery, BatchDropdownResponse>
    {
        private readonly IDropdownRepository _dropdownRepository;
        public GetBatchDropdownsQueryHandler(IDropdownRepository dropdownRepository)
        {
            _dropdownRepository = dropdownRepository;
        }
        public async Task<BatchDropdownResponse> Handle(GetBatchDropdownsQuery query, CancellationToken cancellationToken)
        {
            return await _dropdownRepository.GetBatchDropdownsAsync(query.Request);
        }
    }
}

