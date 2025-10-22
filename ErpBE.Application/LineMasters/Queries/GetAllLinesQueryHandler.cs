using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.LineMasters.Queries
{
    public class GetAllLinesQueryHandler : IRequestHandler<GetAllLinesQuery, List<LineDto>>
    {
        private readonly ILineRepository _repository;

        public GetAllLinesQueryHandler(ILineRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<LineDto>> Handle(GetAllLinesQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllAsync();
        }
    }

}
