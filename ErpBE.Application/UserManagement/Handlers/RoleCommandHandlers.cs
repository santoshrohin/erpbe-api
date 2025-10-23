using ErpBE.Application.UserManagement.Commands;
using ErpBE.Application.DTOs;

using MediatR;

namespace ErpBE.Application.UserManagement.Handlers
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
    {
        private readonly IUserManagementService _userService;

        public CreateRoleCommandHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            return await _userService.CreateRoleAsync(request.Request);
        }
    }

    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleDto>
    {
        private readonly IUserManagementService _userService;

        public UpdateRoleCommandHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<RoleDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            return await _userService.UpdateRoleAsync(request.Request);
        }
    }

    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, bool>
    {
        private readonly IUserManagementService _userService;

        public DeleteRoleCommandHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            return await _userService.DeleteRoleAsync(request.RoleId);
        }
    }
}

