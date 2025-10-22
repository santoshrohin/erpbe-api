namespace ErpBE.Domain.Auth
{
    public interface ILoginRepository
    {
        Task<LoginResult> VerifyLoginAsync(string username, string password, int companyId, int financialYearCode);
    }
}
