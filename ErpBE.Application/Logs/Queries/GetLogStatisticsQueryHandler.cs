using ErpBE.Application.Logs.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.Logs.Queries
{
    public class GetLogStatisticsQueryHandler : IRequestHandler<GetLogStatisticsQuery, LogStatisticsResult>
    {
        private readonly ILogsRepository _logsRepository;

        public GetLogStatisticsQueryHandler(ILogsRepository logsRepository)
        {
            _logsRepository = logsRepository;
        }

        public async Task<LogStatisticsResult> Handle(GetLogStatisticsQuery request, CancellationToken cancellationToken)
        {
            var (levelStats, dailyStats) = await _logsRepository.GetLogStatisticsAsync();

            return new LogStatisticsResult
            {
                LevelStatistics = levelStats,
                DailyStatistics = dailyStats
            };
        }
    }
}




