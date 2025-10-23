using ErpBE.Application.DTOs;
using ErpBE.Application.Common.Models;
using ErpBE.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.UserManagement
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserManagementRepository _userRepository;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(IUserManagementRepository userRepository, ILogger<UserManagementService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        // User Management
        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                _logger.LogInformation("Creating user: {Username}", request.Username);

                // Validate username uniqueness
                if (await _userRepository.UsernameExistsAsync(request.Username))
                {
                    throw new InvalidOperationException($"Username '{request.Username}' already exists");
                }

                // Validate email uniqueness if provided
                if (!string.IsNullOrEmpty(request.Email) && await _userRepository.EmailExistsAsync(request.Email))
                {
                    throw new InvalidOperationException($"Email '{request.Email}' already exists");
                }

                // Create user
                var userId = await _userRepository.CreateUserAsync(request);

                // Assign roles if provided
                if (request.Roles.Any())
                {
                    await _userRepository.AssignRolesToUserAsync(userId, request.Roles);
                }

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new InvalidOperationException("Failed to retrieve created user");
                }

                _logger.LogInformation("User created successfully: {Username} with ID {UserId}", request.Username, userId);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", request.Username);
                throw;
            }
        }

        public async Task<UserDto> UpdateUserAsync(UpdateUserRequest request)
        {
            try
            {
                _logger.LogInformation("Updating user: {UserId}", request.UserId);

                var existingUser = await _userRepository.GetUserByIdAsync(request.UserId);
                if (existingUser == null)
                {
                    throw new InvalidOperationException($"User with ID {request.UserId} not found");
                }

                // Validate email uniqueness if changing email
                if (!string.IsNullOrEmpty(request.Email) && 
                    request.Email != existingUser.Email && 
                    await _userRepository.EmailExistsAsync(request.Email))
                {
                    throw new InvalidOperationException($"Email '{request.Email}' already exists");
                }

                var success = await _userRepository.UpdateUserAsync(request);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to update user");
                }

                // Update roles if provided
                if (request.Roles != null)
                {
                    await _userRepository.AssignRolesToUserAsync(request.UserId, request.Roles);
                }

                var updatedUser = await _userRepository.GetUserByIdAsync(request.UserId);
                if (updatedUser == null)
                {
                    throw new InvalidOperationException("Failed to retrieve updated user");
                }

                _logger.LogInformation("User updated successfully: {UserId}", request.UserId);
                return updatedUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", request.UserId);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Deleting user: {UserId}", userId);

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new InvalidOperationException($"User with ID {userId} not found");
                }

                var success = await _userRepository.DeleteUserAsync(userId);
                if (success)
                {
                    _logger.LogInformation("User deleted successfully: {UserId}", userId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _userRepository.GetUserByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _userRepository.GetUserByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username: {Username}", username);
                throw;
            }
        }

        public async Task<PagedResponse<UserDto>> GetUsersAsync(QueryParameters parameters)
        {
            try
            {
                // This would need to be implemented in the repository
                // For now, return all users with basic pagination
                var allUsers = await _userRepository.GetAllUsersAsync();
                
                var filteredUsers = allUsers.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(parameters.SearchTerm))
                {
                    var searchTerm = parameters.SearchTerm.ToLower();
                    filteredUsers = filteredUsers.Where(u => 
                        u.Username.ToLower().Contains(searchTerm) ||
                        u.Name.ToLower().Contains(searchTerm) ||
                        (u.Email != null && u.Email.ToLower().Contains(searchTerm)));
                }

                // Apply filters
                if (parameters.Filters != null)
                {
                    if (parameters.Filters.ContainsKey("IsActive"))
                    {
                        var isActive = bool.Parse(parameters.Filters["IsActive"]);
                        filteredUsers = filteredUsers.Where(u => u.IsActive == isActive);
                    }
                    if (parameters.Filters.ContainsKey("CompanyId"))
                    {
                        var companyId = int.Parse(parameters.Filters["CompanyId"]);
                        filteredUsers = filteredUsers.Where(u => u.CompanyId == companyId);
                    }
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(parameters.SortBy))
                {
                    var property = typeof(UserDto).GetProperty(parameters.SortBy);
                    if (property != null)
                    {
                        if (parameters.SortDirection?.ToLower() == "desc")
                        {
                            filteredUsers = filteredUsers.OrderByDescending(x => property.GetValue(x));
                        }
                        else
                        {
                            filteredUsers = filteredUsers.OrderBy(x => property.GetValue(x));
                        }
                    }
                }

                var totalCount = filteredUsers.Count();
                var pagedUsers = filteredUsers
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToList();

                return new PagedResponse<UserDto>(pagedUsers, totalCount, parameters.PageNumber, parameters.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with pagination");
                throw;
            }
        }

        public async Task<List<UserDto>> GetUsersByCompanyAsync(int companyId)
        {
            try
            {
                return await _userRepository.GetUsersByCompanyAsync(companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by company: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
        {
            try
            {
                _logger.LogInformation("Changing password for user: {UserId}", request.UserId);

                var user = await _userRepository.GetUserByIdAsync(request.UserId);
                if (user == null)
                {
                    throw new InvalidOperationException($"User with ID {request.UserId} not found");
                }

                var success = await _userRepository.ChangePasswordAsync(request.UserId, request.NewPassword);
                if (success)
                {
                    _logger.LogInformation("Password changed successfully for user: {UserId}", request.UserId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", request.UserId);
                throw;
            }
        }

        public async Task<bool> ActivateUserAsync(int userId, bool isActive)
        {
            try
            {
                _logger.LogInformation("Activating/Deactivating user: {UserId} to {IsActive}", userId, isActive);

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new InvalidOperationException($"User with ID {userId} not found");
                }

                var success = await _userRepository.ActivateUserAsync(userId, isActive);
                if (success)
                {
                    _logger.LogInformation("User activation status changed successfully: {UserId} to {IsActive}", userId, isActive);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing activation status for user: {UserId}", userId);
                throw;
            }
        }

        // Role Management
        public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
        {
            try
            {
                _logger.LogInformation("Creating role: {RoleName}", request.RoleName);

                if (await _userRepository.RoleExistsAsync(request.RoleName))
                {
                    throw new InvalidOperationException($"Role '{request.RoleName}' already exists");
                }

                var roleId = await _userRepository.CreateRoleAsync(request);
                var role = await _userRepository.GetRoleByIdAsync(roleId);
                if (role == null)
                {
                    throw new InvalidOperationException("Failed to retrieve created role");
                }

                _logger.LogInformation("Role created successfully: {RoleName} with ID {RoleId}", request.RoleName, roleId);
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role: {RoleName}", request.RoleName);
                throw;
            }
        }

        public async Task<RoleDto> UpdateRoleAsync(UpdateRoleRequest request)
        {
            try
            {
                _logger.LogInformation("Updating role: {RoleId}", request.RoleId);

                var existingRole = await _userRepository.GetRoleByIdAsync(request.RoleId);
                if (existingRole == null)
                {
                    throw new InvalidOperationException($"Role with ID {request.RoleId} not found");
                }

                var success = await _userRepository.UpdateRoleAsync(request);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to update role");
                }

                var updatedRole = await _userRepository.GetRoleByIdAsync(request.RoleId);
                if (updatedRole == null)
                {
                    throw new InvalidOperationException("Failed to retrieve updated role");
                }

                _logger.LogInformation("Role updated successfully: {RoleId}", request.RoleId);
                return updatedRole;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role: {RoleId}", request.RoleId);
                throw;
            }
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            try
            {
                _logger.LogInformation("Deleting role: {RoleId}", roleId);

                var role = await _userRepository.GetRoleByIdAsync(roleId);
                if (role == null)
                {
                    throw new InvalidOperationException($"Role with ID {roleId} not found");
                }

                var success = await _userRepository.DeleteRoleAsync(roleId);
                if (success)
                {
                    _logger.LogInformation("Role deleted successfully: {RoleId}", roleId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role: {RoleId}", roleId);
                throw;
            }
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int roleId)
        {
            try
            {
                return await _userRepository.GetRoleByIdAsync(roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by ID: {RoleId}", roleId);
                throw;
            }
        }

        public async Task<RoleDto?> GetRoleByNameAsync(string roleName)
        {
            try
            {
                return await _userRepository.GetRoleByNameAsync(roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by name: {RoleName}", roleName);
                throw;
            }
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            try
            {
                return await _userRepository.GetAllRolesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                throw;
            }
        }

        public async Task<List<RoleDto>> GetActiveRolesAsync()
        {
            try
            {
                return await _userRepository.GetActiveRolesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active roles");
                throw;
            }
        }

        // User-Role Assignment
        public async Task<bool> AssignRolesToUserAsync(AssignRolesRequest request)
        {
            try
            {
                _logger.LogInformation("Assigning roles to user: {UserId}, Roles: {Roles}", request.UserId, string.Join(",", request.Roles));

                var user = await _userRepository.GetUserByIdAsync(request.UserId);
                if (user == null)
                {
                    throw new InvalidOperationException($"User with ID {request.UserId} not found");
                }

                var success = await _userRepository.AssignRolesToUserAsync(request.UserId, request.Roles);
                if (success)
                {
                    _logger.LogInformation("Roles assigned successfully to user: {UserId}", request.UserId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning roles to user: {UserId}", request.UserId);
                throw;
            }
        }

        public async Task<bool> RemoveRolesFromUserAsync(int userId, List<string> roles)
        {
            try
            {
                _logger.LogInformation("Removing roles from user: {UserId}, Roles: {Roles}", userId, string.Join(",", roles));

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new InvalidOperationException($"User with ID {userId} not found");
                }

                var success = await _userRepository.RemoveRolesFromUserAsync(userId, roles);
                if (success)
                {
                    _logger.LogInformation("Roles removed successfully from user: {UserId}", userId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing roles from user: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            try
            {
                return await _userRepository.GetUserRolesAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<UserDto>> GetUsersByRoleAsync(string roleName)
        {
            try
            {
                return await _userRepository.GetUsersByRoleAsync(roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role: {RoleName}", roleName);
                throw;
            }
        }

        // Validation
        public async Task<bool> UsernameExistsAsync(string username)
        {
            try
            {
                return await _userRepository.UsernameExistsAsync(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username existence: {Username}", username);
                throw;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                return await _userRepository.EmailExistsAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence: {Email}", email);
                throw;
            }
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            try
            {
                return await _userRepository.RoleExistsAsync(roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role existence: {RoleName}", roleName);
                throw;
            }
        }
    }
}



