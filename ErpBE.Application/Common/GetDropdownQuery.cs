using ErpBE.Domain.CommonDto;
using MediatR;

namespace ErpBE.Application.Common
{
    public class GetDropdownQuery : IRequest<List<DropdownItem>>
    {
        public DropdownRequest Request { get; }

        public GetDropdownQuery(DropdownRequest request)
        {
            Request = request;
        }
    }
}
