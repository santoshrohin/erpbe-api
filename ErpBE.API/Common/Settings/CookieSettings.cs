namespace ErpBE.API.Common.Settings;

public class CookieSettings
{
    public const string SectionName = "CookieSettings";

    public string RefreshTokenCookieName { get; set; } = "erpbe_rt";
    public bool   Secure                 { get; set; } = false;
    public string SameSite               { get; set; } = "Lax";
    public int    ExpiryDays             { get; set; } = 7;
}
