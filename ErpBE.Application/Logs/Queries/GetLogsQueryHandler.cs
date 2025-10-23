using ErpBE.Application.Logs.DTOs;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.Logs.Queries
{
    public class GetLogsQueryHandler : IRequestHandler<GetLogsQuery, PagedLogsResult>
    {
        private readonly ILogsRepository _logsRepository;

        public GetLogsQueryHandler(ILogsRepository logsRepository)
        {
            _logsRepository = logsRepository;
        }

        public async Task<PagedLogsResult> Handle(GetLogsQuery request, CancellationToken cancellationToken)
        {
            var parameters = request.QueryParameters;

            var (logs, totalCount) = await _logsRepository.GetLogsAsync(
                parameters.PageNumber,
                parameters.PageSize,
                parameters.Level,
                parameters.SearchTerm,
                parameters.StartDate,
                parameters.EndDate);

            var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);

            var logDtos = logs.Select(log => new LogDto
            {
                Id = log.Id,
                TimeStamp = log.TimeStamp,
                Level = log.Level,
                Message = log.Message,
                Exception = log.Exception,
                Properties = log.Properties,
                UserId = log.UserId,
                RequestId = log.RequestId,
                ActionName = log.ActionName
            });

            return new PagedLogsResult
            {
                Data = logDtos,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = parameters.PageNumber > 1,
                HasNextPage = parameters.PageNumber < totalPages
            };
        }
    }
}

