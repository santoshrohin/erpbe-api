using ErpBE.Application.UserManagement.Commands;
using ErpBE.Application.DTOs;

using MediatR;

namespace ErpBE.Application.UserManagement.Handlers
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        private readonly IUserManagementService _userService;

        public CreateUserCommandHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            return await _userService.CreateUserAsync(request.Request);
        }
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
    {
        private readonly IUserManagementService _userService;

        public UpdateUserCommandHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            return await _userService.UpdateUserAsync(request.Request);
        }
    }

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly IUserManagementService _userService;

        public DeleteUserCommandHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            return await _userService.DeleteUserAsync(request.UserId);
        }
    }

    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        private readonly IUserManagementService _userService;

        public ChangePasswordCommandHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            return await _userService.ChangePasswordAsync(request.Request);
        }
    }

    public class SetUserActiveStatusCommandHandler : IRequestHandler<SetUserActiveStatusCommand, bool>
    {
        private readonly IUserManagementService _userService;

        public SetUserActiveStatusCommandHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<bool> Handle(SetUserActiveStatusCommand request, CancellationToken cancellationToken)
        {
            return await _userService.ActivateUserAsync(request.UserId, request.IsActive);
        }
    }

    public class AssignRolesToUserCommandHandler : IRequestHandler<AssignRolesToUserCommand, bool>
    {
        private readonly IUserManagementService _userService;

        public AssignRolesToUserCommandHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<bool> Handle(AssignRolesToUserCommand request, CancellationToken cancellationToken)
        {
            return await _userService.AssignRolesToUserAsync(request.Request);
        }
    }

    public class RemoveRolesFromUserCommandHandler : IRequestHandler<RemoveRolesFromUserCommand, bool>
    {
        private readonly IUserManagementService _userService;

        public RemoveRolesFromUserCommandHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<bool> Handle(RemoveRolesFromUserCommand request, CancellationToken cancellationToken)
        {
            return await _userService.RemoveRolesFromUserAsync(request.UserId, request.Roles);
        }
    }
}

