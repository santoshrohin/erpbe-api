using ErpBE.Application.UserManagement.Queries;
using ErpBE.Application.DTOs;
using ErpBE.Application.Common.Models;

using MediatR;

namespace ErpBE.Application.UserManagement.Handlers
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        private readonly IUserManagementService _userService;

        public GetUserByIdQueryHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetUserByIdAsync(request.UserId);
        }
    }

    public class GetUserByUsernameQueryHandler : IRequestHandler<GetUserByUsernameQuery, UserDto?>
    {
        private readonly IUserManagementService _userService;

        public GetUserByUsernameQueryHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<UserDto?> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetUserByUsernameAsync(request.Username);
        }
    }

    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResponse<UserDto>>
    {
        private readonly IUserManagementService _userService;

        public GetUsersQueryHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<PagedResponse<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetUsersAsync(request.QueryParameters);
        }
    }

    public class GetUsersByCompanyQueryHandler : IRequestHandler<GetUsersByCompanyQuery, List<UserDto>>
    {
        private readonly IUserManagementService _userService;

        public GetUsersByCompanyQueryHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<List<UserDto>> Handle(GetUsersByCompanyQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetUsersByCompanyAsync(request.CompanyId);
        }
    }

    public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, List<string>>
    {
        private readonly IUserManagementService _userService;

        public GetUserRolesQueryHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<List<string>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetUserRolesAsync(request.UserId);
        }
    }

    public class GetUsersByRoleQueryHandler : IRequestHandler<GetUsersByRoleQuery, List<UserDto>>
    {
        private readonly IUserManagementService _userService;

        public GetUsersByRoleQueryHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<List<UserDto>> Handle(GetUsersByRoleQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetUsersByRoleAsync(request.RoleName);
        }
    }

    public class CheckUsernameExistsQueryHandler : IRequestHandler<CheckUsernameExistsQuery, bool>
    {
        private readonly IUserManagementService _userService;

        public CheckUsernameExistsQueryHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<bool> Handle(CheckUsernameExistsQuery request, CancellationToken cancellationToken)
        {
            return await _userService.UsernameExistsAsync(request.Username);
        }
    }

    public class CheckEmailExistsQueryHandler : IRequestHandler<CheckEmailExistsQuery, bool>
    {
        private readonly IUserManagementService _userService;

        public CheckEmailExistsQueryHandler(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<bool> Handle(CheckEmailExistsQuery request, CancellationToken cancellationToken)
        {
            return await _userService.EmailExistsAsync(request.Email);
        }
    }
}

