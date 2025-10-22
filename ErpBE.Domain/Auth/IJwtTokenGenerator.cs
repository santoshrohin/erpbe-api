namespace ErpBE.Domain.Auth
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(string username, string userId, int companyId, List<string>? roles = null);
    }
}
