using System.ComponentModel.DataAnnotations;

namespace ErpBE.API.Common
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class AuditAttribute : Attribute
    {
        public string EntityName { get; }
        public string? EntityIdProperty { get; set; }
        public bool TrackPropertyChanges { get; set; } = true;
        public bool TrackOldValues { get; set; } = true;
        public bool TrackNewValues { get; set; } = true;
        public string? Description { get; set; }

        public AuditAttribute(string entityName)
        {
            EntityName = entityName;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class NoAuditAttribute : Attribute
    {
        public NoAuditAttribute() { }
    }
}
