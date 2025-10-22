using System.ComponentModel.DataAnnotations;

namespace ErpBE.Domain.Audit
{
    public class AuditEntry
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string EntityId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE
        
        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? UserName { get; set; }
        
        [MaxLength(50)]
        public string? UserRole { get; set; }
        
        [MaxLength(50)]
        public string? CompanyId { get; set; }
        
        [MaxLength(100)]
        public string? IpAddress { get; set; }
        
        [MaxLength(500)]
        public string? UserAgent { get; set; }
        
        [MaxLength(200)]
        public string? Endpoint { get; set; }
        
        [MaxLength(50)]
        public string? HttpMethod { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        // JSON serialized data
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        
        // Navigation property
        public virtual ICollection<AuditPropertyChange> PropertyChanges { get; set; } = new List<AuditPropertyChange>();
    }
}
