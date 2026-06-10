using MediatR;

namespace ErpBE.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<RefreshTokenResult>
{
    public string RawToken    { get; set; } = string.Empty;
    public string? IpAddress  { get; set; }
    public int    ExpiryDays  { get; set; } = 7;
}

public class RefreshTokenResult
{
    public string              NewAccessToken    { get; set; } = string.Empty;
    public string              NewRawToken       { get; set; } = string.Empty;
    public int                 UserCode          { get; set; }
    public string              Username          { get; set; } = string.Empty;
    public int                 CompanyId         { get; set; }
    public int                 FinancialYearCode { get; set; }
    public Dictionary<int,string> Permissions   { get; set; } = new();
}
