namespace ErpBE.Application.Audit.Models
{
    /// <summary>
    /// Configuration for endpoint auditing
    /// </summary>
    public class AuditConfiguration
    {
        public int Id { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string? EntityIdProperty { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool TrackPropertyChanges { get; set; } = true;
        public bool TrackOldValues { get; set; } = true;
        public bool TrackNewValues { get; set; } = true;
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }
}

