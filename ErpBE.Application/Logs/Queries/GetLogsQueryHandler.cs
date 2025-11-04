using ErpBE.Application.Logs.DTOs;
using ErpBE.Application.Interfaces;
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

            // Cap page size at 100 to match stored procedure behavior
            // If pageSize is 0 or negative, default to 50
            var actualPageSize = parameters.PageSize <= 0 ? 50 : Math.Min(parameters.PageSize, 100);

            var (logs, totalCount) = await _logsRepository.GetLogsAsync(
                parameters.PageNumber,
                actualPageSize,
                parameters.Level,
                parameters.SearchTerm,
                parameters.StartDate,
                parameters.EndDate);

            var totalPages = (int)Math.Ceiling((double)totalCount / actualPageSize);

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
                PageSize = actualPageSize,
                TotalPages = totalPages,
                HasPreviousPage = parameters.PageNumber > 1,
                HasNextPage = parameters.PageNumber < totalPages
            };
        }
    }
}
