using ErpBE.Application.UserManagement.Queries;
using ErpBE.Application.DTOs;

using MediatR;

namespace ErpBE.Application.UserManagement.Handlers
{
    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDto?>
    {
        private readonly IUserManagementService _userService;

        public GetRoleByIdQueryHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<RoleDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetRoleByIdAsync(request.RoleId);
        }
    }

    public class GetRoleByNameQueryHandler : IRequestHandler<GetRoleByNameQuery, RoleDto?>
    {
        private readonly IUserManagementService _userService;

        public GetRoleByNameQueryHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<RoleDto?> Handle(GetRoleByNameQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetRoleByNameAsync(request.RoleName);
        }
    }

    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
    {
        private readonly IUserManagementService _userService;

        public GetAllRolesQueryHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<List<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetAllRolesAsync();
        }
    }

    public class GetActiveRolesQueryHandler : IRequestHandler<GetActiveRolesQuery, List<RoleDto>>
    {
        private readonly IUserManagementService _userService;

        public GetActiveRolesQueryHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<List<RoleDto>> Handle(GetActiveRolesQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetActiveRolesAsync();
        }
    }

    public class CheckRoleExistsQueryHandler : IRequestHandler<CheckRoleExistsQuery, bool>
    {
        private readonly IUserManagementService _userService;

        public CheckRoleExistsQueryHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<bool> Handle(CheckRoleExistsQuery request, CancellationToken cancellationToken)
        {
            return await _userService.RoleExistsAsync(request.RoleName);
        }
    }
}

