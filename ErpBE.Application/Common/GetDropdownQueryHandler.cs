using ErpBE.Application.Interfaces;
using ErpBE.Application.Common.Models;
using MediatR;

namespace ErpBE.Application.Common
{
    public class GetDropdownQueryHandler : IRequestHandler<GetDropdownQuery, List<DropdownItem>>
    {
        private readonly IDropdownRepository _repository;

        public GetDropdownQueryHandler(IDropdownRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DropdownItem>> Handle(GetDropdownQuery query, CancellationToken cancellationToken)
        {
            try
            {
                return await _repository.GetDropdownDataAsync(query.Request);
            }
            catch (Exception ex)
            {
                // Log the error and return empty list for invalid tables
                // This allows the controller to return a proper response
                throw new ArgumentException($"Invalid table or query parameters: {ex.Message}", ex);
            }
        }
    }

}
