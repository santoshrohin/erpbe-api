using ErpBE.Domain.CommonDto;

namespace ErpBE.Domain.DTOs
{
    public class AuditEntryDto
    {
        public int Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? UserRole { get; set; }
        public string? CompanyId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Endpoint { get; set; }
        public string? HttpMethod { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Description { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public List<AuditPropertyChangeDto> PropertyChanges { get; set; } = new();
    }

    public class AuditPropertyChangeDto
    {
        public int Id { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? PropertyType { get; set; }
    }

    public class AuditConfigurationDto
    {
        public int Id { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string? EntityIdProperty { get; set; }
        public bool IsEnabled { get; set; }
        public bool TrackPropertyChanges { get; set; }
        public bool TrackOldValues { get; set; }
        public bool TrackNewValues { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class AuditQueryParameters : QueryParameters
    {
        public string? EntityName { get; set; }
        public string? EntityId { get; set; }
        public string? Action { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Endpoint { get; set; }
    }
}
