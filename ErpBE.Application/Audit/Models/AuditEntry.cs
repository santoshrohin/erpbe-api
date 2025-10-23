namespace ErpBE.Application.Audit.Models
{
    /// <summary>
    /// Represents a single audit log entry
    /// </summary>
    public class AuditEntry
    {
        public int Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? UserRole { get; set; }
        public string? CompanyId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Endpoint { get; set; }
        public string? HttpMethod { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public ICollection<AuditPropertyChange> PropertyChanges { get; set; } = new List<AuditPropertyChange>();
    }
}

