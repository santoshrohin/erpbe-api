namespace ErpBE.Application.Auth.Queries.Login
{
    public class LoginResponse
    {
        // Access token — short-lived JWT (sent in response body)
        public string AccessToken       { get; set; } = string.Empty;

        // Refresh token — long-lived (set as httpOnly cookie by controller, NOT stored here)
        // Exposed on this DTO only for the controller to set the cookie; never sent to client as JSON.
        [System.Text.Json.Serialization.JsonIgnore]
        public string RefreshToken      { get; set; } = string.Empty;

        public int    UserCode          { get; set; }
        public string Username          { get; set; } = string.Empty;
        public string DisplayName       { get; set; } = string.Empty;
        public int    CompanyId         { get; set; }
        public int    FinancialYearCode { get; set; }
        public string CompanyName       { get; set; } = string.Empty;
        public string Email             { get; set; } = string.Empty;
        public bool   IsAdmin           { get; set; }
        public string OpeningDate       { get; set; } = string.Empty;
        public string ClosingDate       { get; set; } = string.Empty;

        // Legacy bitmask permissions — key = module code, value = "1111000"
        // Frontend uses these for permission-driven UI
        public Dictionary<int, string> Permissions { get; set; } = new();
    }
}
