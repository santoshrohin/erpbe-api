namespace ErpBE.Application.DTOs;

public class ScreenMasterDto
{
    public int    ScreenCode  { get; set; }
    public string ScreenName  { get; set; } = string.Empty;
    public int    ModuleCode  { get; set; }
}

public class UserRightDto
{
    public int    UserCode    { get; set; }
    public int    ScreenCode  { get; set; }
    public string ScreenName  { get; set; } = string.Empty;
    public int    ModuleCode  { get; set; }
    public string Bitmask     { get; set; } = "0000000";
}

public class UserRightRequest
{
    public int    ScreenCode  { get; set; }
    public string Bitmask     { get; set; } = "0000000";
}

public class SaveUserRightsRequest
{
    public int                  UserCode { get; set; }
    public List<UserRightRequest> Rights { get; set; } = new();
}
