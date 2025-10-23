namespace ErpBE.Application.DTOs
{
    public class AuditTrailDto
    {
        public int AuditId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public int RecordId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? SessionId { get; set; }
    }
}
