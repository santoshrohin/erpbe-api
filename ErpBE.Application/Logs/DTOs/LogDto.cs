namespace ErpBE.Application.Logs.DTOs
{
    public class LogDto
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string? Properties { get; set; }
        public string? UserId { get; set; }
        public string? RequestId { get; set; }
        public string? ActionName { get; set; }
    }
}

