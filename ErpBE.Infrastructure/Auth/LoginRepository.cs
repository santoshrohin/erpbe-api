using Dapper;
using ErpBE.Domain.Auth;
using System.Data;

namespace ErpBE.Infrastructure.Auth
{
    public class LoginRepository : ILoginRepository
    {
        private readonly IDbConnection _db;

        public LoginRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<LoginResult> VerifyLoginAsync(string username, string password, int companyId, int financialYearCode)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserName", username);
            parameters.Add("@Password", password);
            parameters.Add("@CompanyId", companyId.ToString()); // Convert to string as per your SP
            parameters.Add("@CompCode", financialYearCode.ToString()); // Convert to string as per your SP

            var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
                "SP_VerifyLogin",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result == null) return null;

            // Get user roles
            var roles = await GetUserRolesAsync(username, companyId.ToString());

            return new LoginResult
            {
                Username = username,
                CompanyId = result.UM_CM_ID,
                CompanyCode = result.CM_CODE,
                CompanyName = result.CM_NAME,
                Email = result.CM_EMAILID,
                Roles = roles
            };
        }

        private async Task<List<string>> GetUserRolesAsync(string username, string companyId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserName", username);
            parameters.Add("@CompanyId", companyId);

            var roles = await _db.QueryAsync<string>(
                "SP_GetUserRoles",
                parameters,
                commandType: CommandType.StoredProcedure);

            return roles.ToList();
        }
    }
}
