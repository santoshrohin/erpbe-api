using System.ComponentModel.DataAnnotations;

namespace ErpBE.Domain.DTOs
{
    public class CreateUserRequest
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int FinancialYearCode { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsAdmin { get; set; } = false;

        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UpdateUserRequest
    {
        [Required]
        public int UserId { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsAdmin { get; set; }

        public List<string>? Roles { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class AssignRolesRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int CompanyId { get; set; }
        public string FinancialYearCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime? LastLoginDateTime { get; set; }
        public string? IpAddress { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class RoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateRoleRequest
    {
        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateRoleRequest
    {
        [Required]
        public int RoleId { get; set; }

        [StringLength(50)]
        public string? RoleName { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}
