namespace ErpBE.Domain.Interfaces
{
    public interface ILogsRepository
    {
        Task<(IEnumerable<dynamic> logs, int totalCount)> GetLogsAsync(
            int pageNumber,
            int pageSize,
            string? level,
            string? searchTerm,
            DateTime? startDate,
            DateTime? endDate);

        Task<(IEnumerable<dynamic> levelStats, IEnumerable<dynamic> dailyStats)> GetLogStatisticsAsync();

        Task<int> CleanupOldLogsAsync(int daysToKeep);
    }
}

