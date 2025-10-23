using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.UserManagement.Commands
{
    // Create Role Command
    public class CreateRoleCommand : IRequest<RoleDto>
    {
        public CreateRoleRequest Request { get; set; } = new();
    }

    // Update Role Command
    public class UpdateRoleCommand : IRequest<RoleDto>
    {
        public UpdateRoleRequest Request { get; set; } = new();
    }

    // Delete Role Command
    public class DeleteRoleCommand : IRequest<bool>
    {
        public int RoleId { get; set; }
    }
}

