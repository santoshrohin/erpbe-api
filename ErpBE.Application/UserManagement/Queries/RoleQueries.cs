using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.UserManagement.Queries
{
    // Get Role By ID Query
    public class GetRoleByIdQuery : IRequest<RoleDto?>
    {
        public int RoleId { get; set; }
    }

    // Get Role By Name Query
    public class GetRoleByNameQuery : IRequest<RoleDto?>
    {
        public string RoleName { get; set; } = string.Empty;
    }

    // Get All Roles Query
    public class GetAllRolesQuery : IRequest<List<RoleDto>>
    {
    }

    // Get Active Roles Query
    public class GetActiveRolesQuery : IRequest<List<RoleDto>>
    {
    }

    // Check Role Exists Query
    public class CheckRoleExistsQuery : IRequest<bool>
    {
        public string RoleName { get; set; } = string.Empty;
    }
}

