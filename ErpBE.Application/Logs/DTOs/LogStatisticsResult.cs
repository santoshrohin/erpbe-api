namespace ErpBE.Application.Logs.DTOs
{
    public class LogStatisticsResult
    {
        public IEnumerable<dynamic> LevelStatistics { get; set; } = new List<dynamic>();
        public IEnumerable<dynamic> DailyStatistics { get; set; } = new List<dynamic>();
    }
}

