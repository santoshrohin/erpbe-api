namespace ErpBE.Domain.Common
{
    public class UserRole
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RoleDescription { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
