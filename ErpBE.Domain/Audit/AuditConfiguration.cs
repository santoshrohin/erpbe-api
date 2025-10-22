using System.ComponentModel.DataAnnotations;

namespace ErpBE.Domain.Audit
{
    public class AuditConfiguration
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Endpoint { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(10)]
        public string HttpMethod { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? EntityIdProperty { get; set; }
        
        public bool IsEnabled { get; set; } = true;
        
        public bool TrackPropertyChanges { get; set; } = true;
        
        public bool TrackOldValues { get; set; } = true;
        
        public bool TrackNewValues { get; set; } = true;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
    }
}
