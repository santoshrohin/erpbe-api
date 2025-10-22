using System.ComponentModel.DataAnnotations;

namespace ErpBE.Domain.Audit
{
    public class AuditPropertyChange
    {
        [Key]
        public int Id { get; set; }
        
        public int AuditEntryId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string PropertyName { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? OldValue { get; set; }
        
        [MaxLength(500)]
        public string? NewValue { get; set; }
        
        [MaxLength(50)]
        public string? PropertyType { get; set; }
        
        // Navigation property
        public virtual AuditEntry AuditEntry { get; set; } = null!;
    }
}
