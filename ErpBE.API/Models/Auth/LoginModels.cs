namespace ErpBE.API.Models.Auth;

public class LoginClientResponse
{
    public string                 AccessToken       { get; set; } = string.Empty;
    public int                    UserCode          { get; set; }
    public string                 Username          { get; set; } = string.Empty;
    public string                 DisplayName       { get; set; } = string.Empty;
    public int                    CompanyId         { get; set; }
    public int                    FinancialYearCode { get; set; }
    public string                 CompanyName       { get; set; } = string.Empty;
    public string                 Email             { get; set; } = string.Empty;
    public bool                   IsAdmin           { get; set; }
    public string                 OpeningDate       { get; set; } = string.Empty;
    public string                 ClosingDate       { get; set; } = string.Empty;
    public Dictionary<int,string> Permissions       { get; set; } = new();
}

public class RefreshResponse
{
    public string                 AccessToken       { get; set; } = string.Empty;
    public int                    UserCode          { get; set; }
    public string                 Username          { get; set; } = string.Empty;
    public int                    CompanyId         { get; set; }
    public int                    FinancialYearCode { get; set; }
    public Dictionary<int,string> Permissions       { get; set; } = new();
}
