using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using Dapper;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ErpBE.Infrastructure.Repositories
{
    public class UserManagementRepository : IUserManagementRepository
    {
        private readonly string _connectionString;

        public UserManagementRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found");
        }

        // User Management
        public async Task<int> CreateUserAsync(CreateUserRequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@Username", request.Username);
            parameters.Add("@Password", request.Password); // Note: In production, hash this password
            parameters.Add("@Name", request.Name);
            parameters.Add("@Email", request.Email);
            parameters.Add("@CompanyId", request.CompanyId);
            parameters.Add("@FinancialYearCode", request.FinancialYearCode);
            parameters.Add("@IsActive", request.IsActive);
            parameters.Add("@IsAdmin", request.IsAdmin);

            var userId = await connection.QuerySingleAsync<int>("SP_CreateUser", parameters, commandType: System.Data.CommandType.StoredProcedure);
            
            return userId;
        }

        public async Task<bool> UpdateUserAsync(UpdateUserRequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", request.UserId);
            parameters.Add("@Name", request.Name);
            parameters.Add("@Email", request.Email);
            parameters.Add("@IsActive", request.IsActive);

            var result = await connection.ExecuteAsync("SP_UpdateUser", parameters, commandType: System.Data.CommandType.StoredProcedure);
            return result > 0;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            var result = await connection.ExecuteAsync("SP_DeleteUser", parameters, commandType: System.Data.CommandType.StoredProcedure);
            return result > 0;
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            var user = await connection.QueryFirstOrDefaultAsync<UserDto>("SP_GetUserById", parameters, commandType: System.Data.CommandType.StoredProcedure);
            
            if (user != null)
            {
                user.Roles = await GetUserRolesAsync(userId);
            }

            return user;
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@Username", username);

            var user = await connection.QueryFirstOrDefaultAsync<UserDto>("SP_GetUserByUsername", parameters, commandType: System.Data.CommandType.StoredProcedure);
            
            if (user != null)
            {
                user.Roles = await GetUserRolesAsync(user.UserId);
            }

            return user;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            
            var users = await connection.QueryAsync<UserDto>("SP_GetAllUsers", commandType: System.Data.CommandType.StoredProcedure);
            
            var userList = users.ToList();
            
            // Get roles for each user
            foreach (var user in userList)
            {
                user.Roles = await GetUserRolesAsync(user.UserId);
            }

            return userList;
        }

        public async Task<List<UserDto>> GetUsersByCompanyAsync(int companyId)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", companyId);

            var users = await connection.QueryAsync<UserDto>("SP_GetUsersByCompany", parameters, commandType: System.Data.CommandType.StoredProcedure);
            
            var userList = users.ToList();
            
            // Get roles for each user
            foreach (var user in userList)
            {
                user.Roles = await GetUserRolesAsync(user.UserId);
            }

            return userList;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@NewPassword", newPassword); // Note: In production, hash this password

            var result = await connection.ExecuteAsync("SP_ChangePassword", parameters, commandType: System.Data.CommandType.StoredProcedure);
            return result > 0;
        }

        public async Task<bool> ActivateUserAsync(int userId, bool isActive)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@IsActive", isActive);

            var result = await connection.ExecuteAsync("SP_ActivateUser", parameters, commandType: System.Data.CommandType.StoredProcedure);
            return result > 0;
        }

        // Role Management
        public async Task<int> CreateRoleAsync(CreateRoleRequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@RoleName", request.RoleName);
            parameters.Add("@RoleDescription", request.Description);
            parameters.Add("@IsActive", request.IsActive);

            var roleId = await connection.QuerySingleAsync<int>("SP_CreateRole", parameters, commandType: System.Data.CommandType.StoredProcedure);
            
            return roleId;
        }

        public async Task<bool> UpdateRoleAsync(UpdateRoleRequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@RoleId", request.RoleId);
            parameters.Add("@RoleName", request.RoleName);
            parameters.Add("@RoleDescription", request.Description);
            parameters.Add("@IsActive", request.IsActive);

            var result = await connection.ExecuteAsync("SP_UpdateRole", parameters, commandType: System.Data.CommandType.StoredProcedure);
            return result > 0;
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@RoleId", roleId);

            var result = await connection.ExecuteAsync("SP_DeleteRole", parameters, commandType: System.Data.CommandType.StoredProcedure);
            return result > 0;
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int roleId)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@RoleId", roleId);

            return await connection.QueryFirstOrDefaultAsync<RoleDto>("SP_GetRoleById", parameters, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<RoleDto?> GetRoleByNameAsync(string roleName)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@RoleName", roleName);

            return await connection.QueryFirstOrDefaultAsync<RoleDto>("SP_GetRoleByName", parameters, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            
            var roles = await connection.QueryAsync<RoleDto>("SP_GetAllRoles", commandType: System.Data.CommandType.StoredProcedure);
            return roles.ToList();
        }

        public async Task<List<RoleDto>> GetActiveRolesAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            
            var roles = await connection.QueryAsync<RoleDto>("SP_GetActiveRoles", commandType: System.Data.CommandType.StoredProcedure);
            return roles.ToList();
        }

        // User-Role Assignment
        public async Task<bool> AssignRolesToUserAsync(int userId, List<string> roles)
        {
            using var connection = new SqlConnection(_connectionString);
            
            // First, remove all existing roles for the user
            await RemoveAllRolesFromUserAsync(userId);

            // Then assign new roles
            foreach (var roleName in roles)
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@RoleName", roleName);

                await connection.ExecuteAsync("SP_AssignRoleToUser", parameters, commandType: System.Data.CommandType.StoredProcedure);
            }

            return true;
        }

        public async Task<bool> RemoveRolesFromUserAsync(int userId, List<string> roles)
        {
            using var connection = new SqlConnection(_connectionString);
            
            foreach (var roleName in roles)
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@RoleName", roleName);

                await connection.ExecuteAsync("SP_RemoveRoleFromUser", parameters, commandType: System.Data.CommandType.StoredProcedure);
            }

            return true;
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            
            // First get the username and company ID for this user
            var userInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT UM_USERNAME, UM_CM_ID FROM USER_MASTER WHERE UM_CODE = @UserId",
                new { UserId = userId });

            if (userInfo == null)
                return new List<string>();

            var parameters = new DynamicParameters();
            parameters.Add("@UserName", userInfo.UM_USERNAME);
            parameters.Add("@CompanyId", userInfo.UM_CM_ID.ToString());

            var roles = await connection.QueryAsync<string>("SP_GetUserRoles", parameters, commandType: System.Data.CommandType.StoredProcedure);
            return roles.ToList();
        }

        public async Task<List<UserDto>> GetUsersByRoleAsync(string roleName)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@RoleName", roleName);

            var users = await connection.QueryAsync<UserDto>("SP_GetUsersByRole", parameters, commandType: System.Data.CommandType.StoredProcedure);
            
            var userList = users.ToList();
            
            // Get roles for each user
            foreach (var user in userList)
            {
                user.Roles = await GetUserRolesAsync(user.UserId);
            }

            return userList;
        }

        // Validation
        public async Task<bool> UsernameExistsAsync(string username)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@Username", username);

            var count = await connection.QuerySingleAsync<int>("SP_CheckUsernameExists", parameters, commandType: System.Data.CommandType.StoredProcedure);
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@Email", email);

            var count = await connection.QuerySingleAsync<int>("SP_CheckEmailExists", parameters, commandType: System.Data.CommandType.StoredProcedure);
            return count > 0;
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@RoleName", roleName);

            var count = await connection.QuerySingleAsync<int>("SP_CheckRoleExists", parameters, commandType: System.Data.CommandType.StoredProcedure);
            return count > 0;
        }

        // Helper method
        private async Task RemoveAllRolesFromUserAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            await connection.ExecuteAsync("SP_RemoveAllRolesFromUser", parameters, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
