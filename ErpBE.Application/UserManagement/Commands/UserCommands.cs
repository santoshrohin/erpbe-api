using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.UserManagement.Commands
{
    // Create User Command
    public class CreateUserCommand : IRequest<UserDto>
    {
        public CreateUserRequest Request { get; set; } = new();
    }

    // Update User Command
    public class UpdateUserCommand : IRequest<UserDto>
    {
        public UpdateUserRequest Request { get; set; } = new();
    }

    // Delete User Command
    public class DeleteUserCommand : IRequest<bool>
    {
        public int UserId { get; set; }
    }

    // Change Password Command
    public class ChangePasswordCommand : IRequest<bool>
    {
        public ChangePasswordRequest Request { get; set; } = new();
    }

    // Activate/Deactivate User Command
    public class SetUserActiveStatusCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public bool IsActive { get; set; }
    }

    // Assign Roles Command
    public class AssignRolesToUserCommand : IRequest<bool>
    {
        public AssignRolesRequest Request { get; set; } = new();
    }

    // Remove Roles Command
    public class RemoveRolesFromUserCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}

