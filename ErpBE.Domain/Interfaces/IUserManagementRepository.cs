using ErpBE.Domain.DTOs;

namespace ErpBE.Domain.Interfaces
{
    public interface IUserManagementRepository
    {
        // User Management
        Task<int> CreateUserAsync(CreateUserRequest request);
        Task<bool> UpdateUserAsync(UpdateUserRequest request);
        Task<bool> DeleteUserAsync(int userId);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<List<UserDto>> GetUsersByCompanyAsync(int companyId);
        Task<bool> ChangePasswordAsync(int userId, string newPassword);
        Task<bool> ActivateUserAsync(int userId, bool isActive);

        // Role Management
        Task<int> CreateRoleAsync(CreateRoleRequest request);
        Task<bool> UpdateRoleAsync(UpdateRoleRequest request);
        Task<bool> DeleteRoleAsync(int roleId);
        Task<RoleDto?> GetRoleByIdAsync(int roleId);
        Task<RoleDto?> GetRoleByNameAsync(string roleName);
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<List<RoleDto>> GetActiveRolesAsync();

        // User-Role Assignment
        Task<bool> AssignRolesToUserAsync(int userId, List<string> roles);
        Task<bool> RemoveRolesFromUserAsync(int userId, List<string> roles);
        Task<List<string>> GetUserRolesAsync(int userId);
        Task<List<UserDto>> GetUsersByRoleAsync(string roleName);

        // Validation
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> RoleExistsAsync(string roleName);
    }
}
