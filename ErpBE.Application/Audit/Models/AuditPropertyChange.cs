namespace ErpBE.Application.Audit.Models
{
    /// <summary>
    /// Represents a property-level change in an audit entry
    /// </summary>
    public class AuditPropertyChange
    {
        public int Id { get; set; }
        public int AuditEntryId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? PropertyType { get; set; }
        public AuditEntry AuditEntry { get; set; } = null!;
    }
}

