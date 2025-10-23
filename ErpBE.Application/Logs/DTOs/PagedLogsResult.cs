namespace ErpBE.Application.Logs.DTOs
{
    public class PagedLogsResult
    {
        public IEnumerable<LogDto> Data { get; set; } = new List<LogDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}

