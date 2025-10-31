using ErpBE.Application.Common.Models;
using MediatR;

namespace ErpBE.Application.Common
{
    public class GetBatchDropdownsQuery : IRequest<BatchDropdownResponse>
    {
        public BatchDropdownRequest Request { get; }
        public GetBatchDropdownsQuery(BatchDropdownRequest request)
        {
            Request = request;
        }
    }
}

