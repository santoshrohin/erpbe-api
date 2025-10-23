using ErpBE.Application.DTOs;
using ErpBE.Application.Common.Models;
using MediatR;

namespace ErpBE.Application.UserManagement.Queries
{
    // Get User By ID Query
    public class GetUserByIdQuery : IRequest<UserDto?>
    {
        public int UserId { get; set; }
    }

    // Get User By Username Query
    public class GetUserByUsernameQuery : IRequest<UserDto?>
    {
        public string Username { get; set; } = string.Empty;
    }

    // Get All Users Query
    public class GetUsersQuery : IRequest<PagedResponse<UserDto>>
    {
        public QueryParameters QueryParameters { get; set; } = new();
    }

    // Get Users By Company Query
    public class GetUsersByCompanyQuery : IRequest<List<UserDto>>
    {
        public int CompanyId { get; set; }
    }

    // Get User Roles Query
    public class GetUserRolesQuery : IRequest<List<string>>
    {
        public int UserId { get; set; }
    }

    // Get Users By Role Query
    public class GetUsersByRoleQuery : IRequest<List<UserDto>>
    {
        public string RoleName { get; set; } = string.Empty;
    }

    // Check Username Exists Query
    public class CheckUsernameExistsQuery : IRequest<bool>
    {
        public string Username { get; set; } = string.Empty;
    }

    // Check Email Exists Query
    public class CheckEmailExistsQuery : IRequest<bool>
    {
        public string Email { get; set; } = string.Empty;
    }
}

