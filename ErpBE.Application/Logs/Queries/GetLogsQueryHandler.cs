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

            // Normalize page number - must be at least 1
            var actualPageNumber = parameters.PageNumber <= 0 ? 1 : parameters.PageNumber;

            // Cap page size at 100 to match stored procedure behavior
            // If pageSize is 0 or negative, default to 50
            var actualPageSize = parameters.PageSize <= 0 ? 50 : Math.Min(parameters.PageSize, 100);

            var (logs, totalCount) = await _logsRepository.GetLogsAsync(
                actualPageNumber,
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
                PageNumber = actualPageNumber,
                PageSize = actualPageSize,
                TotalPages = totalPages,
                HasPreviousPage = actualPageNumber > 1,
                HasNextPage = actualPageNumber < totalPages
            };
        }
    }
}
