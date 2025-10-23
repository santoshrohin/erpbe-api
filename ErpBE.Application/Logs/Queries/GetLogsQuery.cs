using ErpBE.Application.Logs.DTOs;
using MediatR;

namespace ErpBE.Application.Logs.Queries
{
    public class GetLogsQuery : IRequest<PagedLogsResult>
    {
        public LogsQueryParameters QueryParameters { get; set; } = new();
    }
}

