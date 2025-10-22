using ErpBE.Domain.DTOs;
using ErpBE.Domain.CommonDto;

namespace ErpBE.Domain.Interfaces
{
    public interface IUserManagementService
    {
        // User Management
        Task<UserDto> CreateUserAsync(CreateUserRequest request);
        Task<UserDto> UpdateUserAsync(UpdateUserRequest request);
        Task<bool> DeleteUserAsync(int userId);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task<PagedResponse<UserDto>> GetUsersAsync(QueryParameters parameters);
        Task<List<UserDto>> GetUsersByCompanyAsync(int companyId);
        Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
        Task<bool> ActivateUserAsync(int userId, bool isActive);

        // Role Management
        Task<RoleDto> CreateRoleAsync(CreateRoleRequest request);
        Task<RoleDto> UpdateRoleAsync(UpdateRoleRequest request);
        Task<bool> DeleteRoleAsync(int roleId);
        Task<RoleDto?> GetRoleByIdAsync(int roleId);
        Task<RoleDto?> GetRoleByNameAsync(string roleName);
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<List<RoleDto>> GetActiveRolesAsync();

        // User-Role Assignment
        Task<bool> AssignRolesToUserAsync(AssignRolesRequest request);
        Task<bool> RemoveRolesFromUserAsync(int userId, List<string> roles);
        Task<List<string>> GetUserRolesAsync(int userId);
        Task<List<UserDto>> GetUsersByRoleAsync(string roleName);

        // Validation
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> RoleExistsAsync(string roleName);
    }
}
